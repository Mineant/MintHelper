using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MioHelper.StatSystem
{
    /// <summary>
    /// Runtime stat: a named modifier multiset plus a monotonic version counter.
    ///
    /// <see cref="ModifierVersion"/> is the heart of the versioned cache — it is bumped on add and
    /// on ACTUAL removal ONLY (a failed remove bumps nothing). It is never reset and never
    /// serialized; the versioned cache compares it to detect stale entries.
    /// </summary>
    public class MioStat
    {
        private readonly List<StatModifier> _statModifiers = new List<StatModifier>();
        private ReadOnlyCollection<StatModifier> _modifiersReadOnly;

        private int _modifierVersion;
        public int ModifierVersion => _modifierVersion;

        public virtual void AddModifier(StatModifier mod)
        {
            if (mod == null) return;
            _statModifiers.Add(mod);
            _modifierVersion++;
            _modifiersReadOnly = null;
        }

        /// <summary>
        /// Removes one matching modifier. Bumps the version only on actual removal.
        /// Without a source, any modifier equal on Value/Type/Order is removed; pass a source to
        /// disambiguate (falls back to scanning for Source == source).
        /// </summary>
        public virtual bool RemoveModifier(StatModifier mod, object source = null)
        {
            if (mod == null) return false;

            if (_statModifiers.Remove(mod))
            {
                _modifierVersion++;
                _modifiersReadOnly = null;
                return true;
            }

            for (int i = 0; i < _statModifiers.Count; i++)
            {
                if (_statModifiers[i].Source == source && _statModifiers[i].Equals(mod))
                {
                    _statModifiers.RemoveAt(i);
                    _modifierVersion++;
                    _modifiersReadOnly = null;
                    return true;
                }
            }

            return false;
        }

        public virtual bool RemoveAllModifiersFromSource(object source)
        {
            int removed = _statModifiers.RemoveAll(m => m.Source == source);
            if (removed > 0)
            {
                _modifierVersion++;
                _modifiersReadOnly = null;
                return true;
            }
            return false;
        }

        /// <summary>Cached read-only wrapper around the live list — reflects later mutations.</summary>
        public virtual IReadOnlyList<StatModifier> GetModifiers()
        {
            if (_modifiersReadOnly == null)
                _modifiersReadOnly = _statModifiers.AsReadOnly();
            return _modifiersReadOnly;
        }

        /// <summary>Raw list — cache build and fold only. Callers must not mutate it.</summary>
        internal IList<StatModifier> RawModifiers => _statModifiers;
    }
}
