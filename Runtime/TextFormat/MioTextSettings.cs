using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MioHelper.TextFormat
{
    /// <summary>
    /// One settings asset for the TextFormat module. Holds the named styles used by
    /// {C:name} / {S:name} spans, and number-format overrides for @{key:name} / {N:name}.
    /// Number formats not defined here fall back to the engine's built-ins
    /// (raw/int/money/pct/pct+/signed/mult/d1/d2); styles have no built-ins — a style must be
    /// defined here to have any visual effect.
    /// </summary>
    [CreateAssetMenu(fileName = "MioTextSettings", menuName = "MioHelper/Text Format/Mio Text Settings")]
    public class MioTextSettings : ScriptableObject
    {
        [Tooltip("Named styles referenced by {C:name} / {S:name} spans. Project-defined — there are no built-in styles.")]
        public List<MioTextStyle> Styles = new List<MioTextStyle>();

        [Tooltip("Number-format overrides for @{key:name} and {N:name}. Names missing here use the engine built-ins (raw, int, money, pct, pct+, signed, mult, d1, d2).")]
        public List<MioTextNumberFormat> Formats = new List<MioTextNumberFormat>();

        /// <summary>Case-insensitive style lookup; null if unknown.</summary>
        public MioTextStyle GetStyle(string name)
        {
            foreach (var style in Styles)
                if (style.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return style;
            return null;
        }

        /// <summary>Case-insensitive format lookup; null if unknown.</summary>
        public MioTextNumberFormat GetFormat(string name)
        {
            foreach (var format in Formats)
                if (format.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return format;
            return null;
        }
    }

    /// <summary>A named character style — color + bold + italic — emitted as TMP rich-text tags.</summary>
    [Serializable]
    public class MioTextStyle
    {
        [Tooltip("Name used in templates: {C:name}...{/C}.")]
        public string Name;
        public Color Color = Color.white;
        [Tooltip("Emit a <color> tag. Disable to use the style for bold/italic only.")]
        public bool UseColorTag = true;
        public bool IsBold;
        public bool IsItalic;

        public string GetOpeningTags()
        {
            string tags = "";
            if (UseColorTag) tags += $"<color=#{ColorUtility.ToHtmlStringRGBA(Color)}>";
            if (IsBold) tags += "<b>";
            if (IsItalic) tags += "<i>";
            return tags;
        }

        public string GetClosingTags()
        {
            string tags = "";
            if (IsItalic) tags += "</i>";
            if (IsBold) tags += "</b>";
            if (UseColorTag) tags += "</color>";
            return tags;
        }

        /// <summary>Wrap text in this style's tags.</summary>
        public string WrapText(string text) => GetOpeningTags() + text + GetClosingTags();
    }

    /// <summary>A named number format: a C# custom numeric format string with an optional
    /// prefix, suffix, and multiplier.</summary>
    [Serializable]
    public class MioTextNumberFormat
    {
        [Tooltip("Name used in templates: @{key:name} / {N:name}.")]
        public string Name;
        [Tooltip("C# custom numeric format string, e.g. \"0.##\", \"+0;-0\".")]
        public string NumberFormat = "0.##";
        public string Prefix = "";
        public string Suffix = "";
        [Tooltip("Applied before formatting — e.g. 100 for percentages.")]
        public float Multiplier = 1f;

        /// <summary>Format a value: (value × Multiplier) → custom format, wrapped in prefix/suffix.</summary>
        public string FormatValue(float value)
        {
            float processed = value * Multiplier;
            string formatted;
            try
            {
                formatted = processed.ToString(NumberFormat, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                formatted = processed.ToString("0.##", CultureInfo.InvariantCulture);
            }
            return Prefix + formatted + Suffix;
        }
    }
}
