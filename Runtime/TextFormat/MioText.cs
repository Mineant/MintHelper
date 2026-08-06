using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MioHelper.TextFormat
{
    /// <summary>
    /// Scene convenience component: holds a template and settings, renders into a
    /// TextMeshProUGUI. Inspector-authored default values support static text; runtime
    /// <see cref="SetValues"/> calls override them (runtime wins). Null settings fall back to
    /// <see cref="MioTextFormatter.DefaultSettings"/>.
    /// </summary>
    [AddComponentMenu("MioHelper/UI/Mio Text")]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class MioText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [Tooltip("Styles + number formats. Null falls back to MioTextFormatter.DefaultSettings.")]
        [SerializeField] private MioTextSettings _settings;
        [Tooltip("Template using @{key}, @{key:fmt}, {C:name}...{/C}, {N:fmt}...{/N}, {L:keyword}...{/L}.")]
        [SerializeField, TextArea(1, 8)] private string _template;
        [Tooltip("Inspector-authored values for static text; runtime SetValues override these.")]
        [SerializeField] private MioTextParameter[] _defaultValues;

        private void Awake()
        {
            if (_text == null) _text = GetComponent<TextMeshProUGUI>();
            Refresh();
        }

        public void SetTemplate(string template) { _template = template; Refresh(); }

        public void SetSettings(MioTextSettings settings) { _settings = settings; Refresh(); }

        /// <summary>Re-render from the current template + default values.</summary>
        public void Refresh()
        {
            if (_text == null) _text = GetComponent<TextMeshProUGUI>();
            if (_text == null) return;
            _text.text = MioTextFormatter.Format(_template, BuildValues(null), _settings);
        }

        /// <summary>Set runtime values (overriding default values) and re-render.</summary>
        public void SetValues(IReadOnlyDictionary<string, object> values)
        {
            if (_text == null) return;
            _text.text = MioTextFormatter.Format(_template, BuildValues(values), _settings);
        }

        /// <summary>Convenience: set a single value and re-render.</summary>
        public void SetValues(string key, object value)
            => SetValues(new Dictionary<string, object> { [key] = value });

        private Dictionary<string, object> BuildValues(IReadOnlyDictionary<string, object> runtime)
        {
            var values = new Dictionary<string, object>();
            if (_defaultValues != null)
                foreach (var parameter in _defaultValues)
                    if (!string.IsNullOrEmpty(parameter.Key)) values[parameter.Key] = parameter.Value;
            if (runtime != null)
                foreach (var kv in runtime) values[kv.Key] = kv.Value;
            return values;
        }
    }

    /// <summary>One inspector-authored template parameter. Values are strings; a numeric string
    /// is parsed when the placeholder carries a format hint (e.g. @{hp:pct}).</summary>
    [System.Serializable]
    public class MioTextParameter
    {
        [Tooltip("Placeholder key, e.g. 'damage' for @{damage}.")]
        public string Key;
        [Tooltip("Value. With @{key:fmt} a numeric string is parsed and formatted.")]
        public string Value;
    }
}
