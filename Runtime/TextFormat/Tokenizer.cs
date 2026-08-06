using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace MioHelper.TextFormat
{
    /// <summary>Per-call state threaded through tokenization: the value map, the settings,
    /// and a recursion guard against self-referential values/templates.</summary>
    internal struct RenderContext
    {
        public IReadOnlyDictionary<string, object> Values;
        public MioTextSettings Settings;
        public int Depth;
    }

    /// <summary>
    /// Single-pass recursive-descent tokenizer for the template grammar. Internal — all public
    /// entry points live on <see cref="MioTextFormatter"/>. One pass, arbitrary cross-family
    /// nesting, case-tolerant closes ({/C} and {/c} both close a {C:...}), re-entrant value
    /// substitution, and verbatim passthrough (with a dev warning) for anything unresolvable.
    /// </summary>
    internal static class Tokenizer
    {
        private const int MaxDepth = 24;

        public static string Render(string template, RenderContext ctx)
        {
            if (template == null) return null;
            if (ctx.Depth > MaxDepth)
            {
                Warn($"recursion limit exceeded — a value or template re-enters itself. Returning verbatim: '{template}'");
                return template;
            }

            var sb = new StringBuilder(template.Length);
            ctx.Depth++;
            RenderTo(template, 0, ctx, sb);
            return sb.ToString();
        }

        private static void RenderTo(string t, int start, RenderContext ctx, StringBuilder sb)
        {
            int i = start;
            int n = t.Length;
            while (i < n)
            {
                char c = t[i];

                // Escapes: \{ \} \@ \\  → literal next char.
                if (c == '\\')
                {
                    if (i + 1 < n)
                    {
                        char next = t[i + 1];
                        if (next == '\\' || next == '{' || next == '}' || next == '@')
                        {
                            sb.Append(next);
                            i += 2;
                            continue;
                        }
                    }
                    sb.Append('\\');
                    i += 1;
                    continue;
                }

                // Placeholder: @{key} or @{key:fmt}.
                if (c == '@' && i + 1 < n && t[i + 1] == '{')
                {
                    if (TryResolvePlaceholder(t, i, ctx, sb, out int consumed))
                    {
                        i += consumed;
                        continue;
                    }
                    sb.Append('@'); // malformed placeholder → literal '@'; the '{' falls through
                    i += 1;
                    continue;
                }

                // Block tag: {C:name}/{S:name}/{N:name}/{L:name} ... {/family}, or a close.
                if (c == '{')
                {
                    if (TryReadOpenTag(t, i, out char family, out string name, out int tagEnd))
                    {
                        if (TryFindClose(t, tagEnd, family, out int closeStart, out int closeEnd))
                        {
                            string inner = t.Substring(tagEnd, closeStart - tagEnd);
                            string renderedInner = Render(inner, ctx);
                            sb.Append(WrapBlock(family, name, renderedInner, ctx));
                            i = closeEnd;
                        }
                        else
                        {
                            Warn($"unclosed tag '{{{family}:{name}}}' — no matching '{{/{family}}}' found; emitting verbatim");
                            sb.Append($"{{{family}:{name}}}");
                            i = tagEnd;
                        }
                        continue;
                    }

                    if (TryReadCloseTag(t, i, out char closeFamily, out int closeTagEnd))
                    {
                        Warn($"unmatched close '{{/{closeFamily}}}' at index {i}; emitting verbatim");
                        sb.Append(t.Substring(i, closeTagEnd - i));
                        i = closeTagEnd;
                        continue;
                    }

                    sb.Append('{'); // not a tag → literal brace
                    i += 1;
                    continue;
                }

                sb.Append(c);
                i += 1;
            }
        }

        #region Placeholders

        private static bool TryResolvePlaceholder(string t, int i, RenderContext ctx, StringBuilder sb, out int consumed)
        {
            int j = i + 2; // skip "@{"
            int keyStart = j;
            while (j < t.Length && t[j] != '}' && t[j] != ':') j++;
            if (j >= t.Length) { consumed = 0; return false; }

            string key = t.Substring(keyStart, j - keyStart);
            if (key.Length == 0) { consumed = 0; return false; } // "@{}"

            string fmt = null;
            int end;
            if (t[j] == ':')
            {
                int fmtStart = j + 1;
                int k = fmtStart;
                while (k < t.Length && t[k] != '}') k++;
                if (k >= t.Length) { consumed = 0; return false; }
                fmt = t.Substring(fmtStart, k - fmtStart);
                end = k + 1;
            }
            else
            {
                end = j + 1;
            }
            consumed = end - i;

            // Control keys are reserved built-ins, resolved before the caller's map.
            if (key.Equals("br", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append('\n');
                return true;
            }

            if (ctx.Values != null && ctx.Values.TryGetValue(key, out object value))
            {
                if (string.IsNullOrEmpty(fmt))
                {
                    AppendResolved(sb, value, ctx);
                }
                else if (TryToNumber(value, out float num))
                {
                    MioTextNumberFormat style = ResolveNumberFormat(ctx, fmt);
                    if (style != null) sb.Append(style.FormatValue(num));
                    else
                    {
                        Warn($"unknown number format '{fmt}' in '@{{{key}}}' — using raw value");
                        AppendResolved(sb, value, ctx);
                    }
                }
                else
                {
                    Warn($"format '{fmt}' ignored in '@{{{key}}}' — value is not numeric");
                    AppendResolved(sb, value, ctx);
                }
                return true;
            }

            Warn($"unknown placeholder key '@{{{key}}}' — emitting verbatim");
            sb.Append(t.Substring(i, end - i));
            return true;
        }

        private static void AppendResolved(StringBuilder sb, object value, RenderContext ctx)
        {
            if (value is string s)
                sb.Append(Render(s, ctx)); // values may contain template tags (re-entrant)
            else
                sb.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        private static bool TryToNumber(object value, out float number)
        {
            switch (value)
            {
                case float f: number = f; return true;
                case double d: number = (float)d; return true;
                case decimal m: number = (float)m; return true;
                case int i: number = i; return true;
                case long l: number = l; return true;
                case short s: number = s; return true;
                case byte b: number = b; return true;
                case string str:
                    return float.TryParse(str.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out number);
                default:
                    number = 0f;
                    return false;
            }
        }

        #endregion

        #region Block tags

        private static bool TryReadOpenTag(string t, int i, out char family, out string name, out int tagEnd)
        {
            family = '\0'; name = null; tagEnd = 0;
            if (i + 2 >= t.Length || t[i + 2] != ':') return false;
            char c = char.ToUpperInvariant(t[i + 1]);
            if (c != 'C' && c != 'S' && c != 'N' && c != 'L') return false;

            int j = i + 3;
            int nameStart = j;
            while (j < t.Length && t[j] != '}') j++;
            if (j >= t.Length || j == nameStart) return false; // no close, or empty name

            family = t[i + 1];
            name = t.Substring(nameStart, j - nameStart);
            tagEnd = j + 1;
            return true;
        }

        private static bool TryReadCloseTag(string t, int i, out char family, out int tagEnd)
        {
            family = '\0'; tagEnd = 0;
            if (i + 3 >= t.Length || t[i + 1] != '/') return false;
            char c = char.ToUpperInvariant(t[i + 2]);
            if ((c != 'C' && c != 'S' && c != 'N' && c != 'L') || t[i + 3] != '}') return false;
            family = t[i + 2];
            tagEnd = i + 4;
            return true;
        }

        /// <summary>Find the close that balances the open at depth 0 of the same family.
        /// C and S are one family (S is an alias of C).</summary>
        private static bool TryFindClose(string t, int searchStart, char family, out int closeStart, out int closeEnd)
        {
            int balance = 0;
            for (int pos = searchStart; pos < t.Length;)
            {
                if (t[pos] == '{')
                {
                    if (TryReadOpenTag(t, pos, out char openFamily, out _, out int openEnd) && SameFamily(openFamily, family))
                    {
                        balance++;
                        pos = openEnd;
                        continue;
                    }
                    if (TryReadCloseTag(t, pos, out char closeFamily, out int closeTagEnd) && SameFamily(closeFamily, family))
                    {
                        balance--;
                        if (balance < 0)
                        {
                            closeStart = pos;
                            closeEnd = closeTagEnd;
                            return true;
                        }
                        pos = closeTagEnd;
                        continue;
                    }
                }
                pos++;
            }
            closeStart = -1;
            closeEnd = -1;
            return false;
        }

        private static bool SameFamily(char a, char b)
        {
            char ua = char.ToUpperInvariant(a);
            char ub = char.ToUpperInvariant(b);
            if (ua == 'C' || ua == 'S') return ub == 'C' || ub == 'S';
            return ua == ub;
        }

        private static string WrapBlock(char family, string name, string inner, RenderContext ctx)
        {
            char upper = char.ToUpperInvariant(family);

            if (upper == 'C' || upper == 'S')
            {
                MioTextStyle style = ctx.Settings?.GetStyle(name);
                if (style != null) return style.GetOpeningTags() + inner + style.GetClosingTags();
                Warn($"unknown style '{name}' in '{{{family}:{name}}}' — emitting verbatim");
                return $"{{{family}:{name}}}" + inner + $"{{/{family}}}";
            }

            if (upper == 'N')
            {
                MioTextNumberFormat fmt = ResolveNumberFormat(ctx, name);
                if (fmt != null)
                {
                    string candidate = inner.Trim();
                    if (candidate.Length > 0
                        && float.TryParse(candidate, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                        return fmt.FormatValue(v);
                    if (candidate.Length == 0) return inner; // empty span — nothing to format
                    Warn($"cannot format '{inner}' as a number in '{{{family}:{name}}}' — leaving content as-is");
                    return inner;
                }
                Warn($"unknown number format '{name}' in '{{{family}:{name}}}' — emitting verbatim");
                return $"{{{family}:{name}}}" + inner + $"{{/{family}}}";
            }

            if (upper == 'L')
                return $"<link=\"{name}\">{inner}</link>";

            return inner;
        }

        private static MioTextNumberFormat ResolveNumberFormat(RenderContext ctx, string name)
            => ctx.Settings?.GetFormat(name) ?? BuiltInNumberFormats.Get(name);

        #endregion

        private static void Warn(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[MioTextFormatter] {message}");
#endif
        }
    }
}
