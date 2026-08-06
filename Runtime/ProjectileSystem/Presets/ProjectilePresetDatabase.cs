using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.ProjectileSystem
{
    /// <summary>
    /// Id-keyed registry of <see cref="ProjectilePresetData"/>. Consumers author one asset and
    /// drop references in, then launch through <see cref="ProjectilePresets"/>. Lookup is lazy
    /// and rebuilt on enable, so reordering the list at edit time needs no extra steps.
    /// </summary>
    [CreateAssetMenu(menuName = "MioHelper/Projectile System/Projectile Preset Database")]
    public class ProjectilePresetDatabase : ScriptableObject
    {
        [SerializeField] private List<ProjectilePresetData> _presets = new();

        private Dictionary<string, ProjectilePresetData> _lookup;

        /// <summary>All presets, in authoring order.</summary>
        public IReadOnlyList<ProjectilePresetData> Presets => _presets;

        private void OnEnable()
        {
            RebuildLookup();
        }

        public ProjectilePresetData Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_lookup == null) RebuildLookup();

            return _lookup.TryGetValue(id, out var preset) ? preset : null;
        }

        public bool Has(string id) => Get(id) != null;

        private void RebuildLookup()
        {
            _lookup = new Dictionary<string, ProjectilePresetData>();
            if (_presets == null) return;

            foreach (var preset in _presets)
            {
                if (preset == null || string.IsNullOrEmpty(preset.Id)) continue;
                if (_lookup.ContainsKey(preset.Id))
                {
                    Debug.LogWarning($"[ProjectilePresetDatabase] Duplicate preset Id '{preset.Id}'. Keeping the first.");
                    continue;
                }
                _lookup.Add(preset.Id, preset);
            }
        }
    }
}
