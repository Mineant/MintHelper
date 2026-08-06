using System;
using System.Collections.Generic;

namespace MioHelper.StatSystem
{
    /// <summary>Runtime stat group: a named collection of <see cref="MioStat"/> instances.</summary>
    public class MioStatGroup
    {
        private readonly Dictionary<string, MioStat> _statTable = new Dictionary<string, MioStat>();

        public virtual bool HasStat(string stat) => stat != null && _statTable.ContainsKey(stat);

        /// <summary>Returns the stat, or null if absent.</summary>
        public virtual MioStat GetStat(string stat) =>
            stat != null && _statTable.TryGetValue(stat, out MioStat s) ? s : null;

        /// <summary>Gets or creates a stat. Presence changes are caught by the versioned cache.</summary>
        public virtual MioStat GetOrAddStat(string stat)
        {
            if (stat == null) throw new ArgumentNullException(nameof(stat));
            if (!_statTable.TryGetValue(stat, out MioStat s))
            {
                s = new MioStat();
                _statTable[stat] = s;
            }
            return s;
        }

        public virtual bool RemoveStat(string stat) => stat != null && _statTable.Remove(stat);

        public virtual IEnumerable<string> StatNames => _statTable.Keys;

        public virtual void AddModifier(string stat, StatModifier mod) =>
            GetOrAddStat(stat).AddModifier(mod);

        public virtual bool RemoveModifier(string stat, StatModifier mod, object source = null)
        {
            MioStat s = GetStat(stat);
            return s != null && s.RemoveModifier(mod, source);
        }

        /// <summary>Removes all modifiers of a source from one stat.</summary>
        public virtual bool RemoveAllModifiersFromSource(string stat, object source)
        {
            MioStat s = GetStat(stat);
            return s != null && s.RemoveAllModifiersFromSource(source);
        }

        /// <summary>Removes all modifiers of a source from every stat in the group.</summary>
        public virtual bool RemoveAllModifiersFromSource(object source)
        {
            bool any = false;
            foreach (MioStat stat in _statTable.Values)
                any |= stat.RemoveAllModifiersFromSource(source);
            return any;
        }
    }
}
