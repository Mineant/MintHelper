using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.TextFormat
{
    /// <summary>
    /// Static entry point for the TextFormat module — a pure tokenizer over a small template
    /// grammar (no state, no scene dependency):
    ///
    ///   @{key}              place a value (letters/digits; e.g. @{damage} or @{401})
    ///   @{key:fmt}          place a value, number-formatted by name (e.g. @{hp:pct})
    ///   {C:name}..{/C}      named character-style span (color/bold/italic); {S:name} is an alias
    ///   {N:fmt}..{/N}       number format applied to a literal or resolved value (e.g. {N:money}100{/N})
    ///   {L:keyword}..{/L}   TMP link span
    ///   \{ \} \@ \\         literal escapes
    ///
    /// Nesting is arbitrary across families ({C:buff}{N:pct+}@{chance}{/N}{/C}), closes are
    /// case-tolerant ({/c} closes {C:...}), control keys like @{br} are built in, and values
    /// that themselves contain template tags are re-tokenized. Anything unresolvable — unknown
    /// key, style, or format — passes through verbatim (a warning is logged in editor/dev builds).
    /// </summary>
    public static class MioTextFormatter
    {
        /// <summary>Convenience default settings for the 2-arg <see cref="Format"/> overload.
        /// Projects typically assign this once at startup (e.g. Resources.Load). Null is fine —
        /// number formats then fall back to the built-ins and styles resolve to nothing.</summary>
        public static MioTextSettings DefaultSettings { get; set; }

        /// <summary>Format using <see cref="DefaultSettings"/>.</summary>
        public static string Format(string template, IReadOnlyDictionary<string, object> values)
            => Format(template, values, DefaultSettings);

        /// <summary>Format with explicit settings. Pure: same inputs, same output.</summary>
        public static string Format(string template, IReadOnlyDictionary<string, object> values, MioTextSettings settings)
        {
            if (template == null) return null;
            return Tokenizer.Render(template, new RenderContext
            {
                Values = values,
                Settings = settings,
                Depth = 0,
            });
        }

        /// <summary>Convenience: format from inline (key, value) pairs using
        /// <see cref="DefaultSettings"/> — e.g.
        /// <c>Format("Cost {N:money}@{price}{/N}", ("price", 120))</c>.</summary>
        public static string Format(string template, params (string Key, object Value)[] parameters)
        {
            var values = new Dictionary<string, object>();
            if (parameters != null)
                foreach (var (key, value) in parameters)
                    values[key] = value;
            return Format(template, values, DefaultSettings);
        }
    }

    /// <summary>
    /// Built-in number formats, used when the caller's <see cref="MioTextSettings"/> does not
    /// define a name. Any of these can be overridden per-project by defining the same name in
    /// the settings asset.
    /// </summary>
    internal static class BuiltInNumberFormats
    {
        private static readonly List<MioTextNumberFormat> _formats = new List<MioTextNumberFormat>
        {
            new MioTextNumberFormat { Name = "raw",    NumberFormat = "0.##",  Suffix = "",  Multiplier = 1f },
            new MioTextNumberFormat { Name = "int",    NumberFormat = "0",     Suffix = "",  Multiplier = 1f },
            new MioTextNumberFormat { Name = "money",  NumberFormat = "0",     Prefix = "$", Suffix = "",   Multiplier = 1f },
            new MioTextNumberFormat { Name = "pct",    NumberFormat = "0.#",   Suffix = "%",              Multiplier = 100f },
            new MioTextNumberFormat { Name = "pct+",   NumberFormat = "+0;-0", Suffix = "%",              Multiplier = 100f },
            new MioTextNumberFormat { Name = "signed", NumberFormat = "+0;-0", Suffix = "",               Multiplier = 1f },
            new MioTextNumberFormat { Name = "mult",   NumberFormat = "0.#",   Prefix = "×", Suffix = "",  Multiplier = 1f },
            new MioTextNumberFormat { Name = "d1",     NumberFormat = "0.0",   Suffix = "",               Multiplier = 1f },
            new MioTextNumberFormat { Name = "d2",     NumberFormat = "0.00",  Suffix = "",               Multiplier = 1f },
        };

        public static MioTextNumberFormat Get(string name)
        {
            foreach (var format in _formats)
                if (format.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) return format;
            return null;
        }
    }
}
