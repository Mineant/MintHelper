using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MioHelper.StatSystem
{
    /// <summary>
    /// Runtime stat table: owns a set of stat groups, an optional chain of parent sheets, and a
    /// per-key versioned cache of resolved modifier lists.
    ///
    /// CACHE CONTRACT (Design A — per-key lazy version validation):
    /// - The cache stores the resolved modifier list per (group-set, stat). The FINAL VALUE is never
    ///   cached — baseValue is injected per call and varies. The SORTED ORDER is never cached —
    ///   StatModifier fields are public and read live by the fold.
    /// - Invalidation is per-key: an entry records the group/stat presence and monotonic stat
    ///   versions it was built from; a read re-validates and rebuilds only the mismatched key.
    /// - Parent-table changes are caught by re-walking the live parent chain and comparing each
    ///   ancestor's reference, structure version, and per-(group,stat) presence + stat version.
    /// - Monotonic counters are never reset (even across <see cref="Release"/>) — see Release().
    /// - Single-threaded Unity main thread only: shared static buffers are used for key building
    ///   and the ancestor walk.
    /// </summary>
    public class MioStatSheet
    {
        // ---- data ----
        private readonly Dictionary<string, MioStatGroup> _statTable = new Dictionary<string, MioStatGroup>();
        private readonly List<MioStatSheet> _parentTables = new List<MioStatSheet>();
        private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
        private ReadOnlyDictionary<string, MioStatGroup> _readOnlyStatTable;

        // Bumped ONLY by AddParentTable/RemoveParentTable. Never reset, never serialized.
        private int _structureVersion = 1;

        private const int MaxAncestors = 10;

        // ---- shared static buffers (single-threaded) ----
        private static string[] s_groupBuffer = new string[8];
        private static readonly StringBuilder s_keyBuilder = new StringBuilder(64);
        private static readonly MioStatSheet[] s_walkVisited = new MioStatSheet[MaxAncestors];

        // ---- cache entry structures (values are replaced whole on rebuild, never mutated in place) ----

        private struct GroupCapture
        {
            public readonly string Group;
            public readonly bool GroupPresent;
            public readonly bool StatPresent;
            public readonly int StatVersion;

            public GroupCapture(string group, bool groupPresent, bool statPresent, int statVersion)
            {
                Group = group;
                GroupPresent = groupPresent;
                StatPresent = statPresent;
                StatVersion = statVersion;
            }
        }

        private struct ChainLink
        {
            public readonly MioStatSheet Table;
            public readonly int StructureVersion;
            public readonly GroupCapture[] Groups;

            public ChainLink(MioStatSheet table, int structureVersion, GroupCapture[] groups)
            {
                Table = table;
                StructureVersion = structureVersion;
                Groups = groups;
            }
        }

        private struct CacheEntry
        {
            public List<StatModifier> Modifiers;
            public ReadOnlyCollection<StatModifier> ReadOnly; // cached wrapper — zero alloc on hits
            public bool IncludesParents;
            public GroupCapture[] SelfGroups;
            public ChainLink[] Chain; // null/empty for leaf entries
        }

        // =======================================================================================
        // Introspection
        // =======================================================================================

        /// <summary>
        /// Read-only view of the sheet's own groups. The view reflects later structural changes.
        /// (Read-only so callers cannot insert pre-populated groups and bypass the versioned cache.)
        /// </summary>
        public virtual IReadOnlyDictionary<string, MioStatGroup> GetStatTable()
        {
            if (_readOnlyStatTable == null)
                _readOnlyStatTable = new ReadOnlyDictionary<string, MioStatGroup>(_statTable);
            return _readOnlyStatTable;
        }

        public virtual MioStatGroup GetGroup(string groupName) =>
            groupName != null && _statTable.TryGetValue(groupName, out MioStatGroup g) ? g : null;

        public virtual MioStatGroup GetOrAddGroup(string groupName)
        {
            if (groupName == null) throw new ArgumentNullException(nameof(groupName));
            if (!_statTable.TryGetValue(groupName, out MioStatGroup g))
            {
                g = new MioStatGroup();
                _statTable[groupName] = g;
            }
            return g;
        }

        public virtual bool RemoveGroup(string groupName) =>
            groupName != null && _statTable.Remove(groupName);

        public virtual bool HasGroup(string groupName) => groupName != null && _statTable.ContainsKey(groupName);

        public virtual bool HasStat(string groupName, string stat)
        {
            MioStatGroup g = GetGroup(groupName);
            return g != null && g.HasStat(stat);
        }

        /// <summary>Number of cached (group-set, stat) entries — introspection/diagnostics only.</summary>
        public virtual int CacheCount => _cache.Count;

        // =======================================================================================
        // Read path
        // =======================================================================================

        /// <summary>Resolved modifiers for one stat across the given groups of THIS sheet only.</summary>
        public virtual IReadOnlyList<StatModifier> GetStatModifiers(IEnumerable<string> groupNames, string stat)
        {
            if (stat == null) throw new ArgumentNullException(nameof(stat));

            int count = CaptureAndSortGroupNames(groupNames, out string[] groups);
            string key = BuildKey(false, groups, count, stat);

            if (_cache.TryGetValue(key, out CacheEntry entry) && !entry.IncludesParents && ValidLeaf(in entry, stat))
                return entry.ReadOnly;

            CacheEntry built = BuildLeaf(groups, count, stat);
            _cache[key] = built;
            return built.ReadOnly;
        }

        /// <summary>Resolved modifiers for one stat across the given groups, including all ancestor sheets.</summary>
        public virtual IReadOnlyList<StatModifier> GetStatModifiersIncludingParent(IEnumerable<string> groupNames, string stat)
            => GetStatModifiersIncludingParent(groupNames, stat, 0);

        // Recursive core. step guards against parent cycles: even though WalkAncestors dedups the
        // walk, the recursion into each ancestor's own parent-inclusive read re-enters the chain, so
        // a cycle (A -> B -> A) must be cut off by depth or it would overflow the stack.
        private IReadOnlyList<StatModifier> GetStatModifiersIncludingParent(IEnumerable<string> groupNames, string stat, int step)
        {
            if (stat == null) throw new ArgumentNullException(nameof(stat));

            if (step > MaxAncestors)
            {
                LogCycle();
                return Array.Empty<StatModifier>();
            }

            int count = CaptureAndSortGroupNames(groupNames, out string[] groups);
            string key = BuildKey(true, groups, count, stat);

            if (_cache.TryGetValue(key, out CacheEntry entry) && entry.IncludesParents && ValidChain(in entry, stat))
                return entry.ReadOnly;

            CacheEntry built = BuildChain(groups, count, stat, step);
            _cache[key] = built;
            return built.ReadOnly;
        }

        /// <summary>
        /// The primary query: folds the resolved modifiers over <paramref name="baseValue"/>.
        /// The fold ALWAYS runs — baseValue never participates in caching.
        /// </summary>
        public virtual float GetTotalStatValue(string stat, IEnumerable<string> groupNames, bool lookupParentTables = true, float baseValue = 0f)
        {
            if (stat == null) throw new ArgumentNullException(nameof(stat));

            IReadOnlyList<StatModifier> mods = lookupParentTables
                ? GetStatModifiersIncludingParent(groupNames, stat)
                : GetStatModifiers(groupNames, stat);

            return StatSystem.CalculateFinalValue(mods, baseValue);
        }

        // =======================================================================================
        // Write path — content mutators bump ONLY per-stat versions, never _structureVersion,
        // and never clear the cache (the versioned entries invalidate themselves on next read).
        // =======================================================================================

        public virtual void AddStatModifier(string groupName, MioStatModifier mod, object source = null)
        {
            if (mod == null) throw new ArgumentNullException(nameof(mod));
            if (mod.Stat == null) throw new ArgumentNullException(nameof(mod.Stat));

            StatModifier runtime = mod.GetStatModifier();
            if (source != null) runtime.Source = source;
            GetOrAddGroup(groupName).AddModifier(runtime.Stat, runtime);
        }

        public virtual bool RemoveStatModifier(string groupName, string stat, StatModifier mod, object source = null)
        {
            MioStatGroup group = GetGroup(groupName);
            return group != null && group.RemoveModifier(stat, mod, source);
        }

        public virtual bool RemoveStatModifier(string groupName, MioStatModifier serializedModifier, object source = null)
        {
            if (serializedModifier == null) return false;

            StatModifier runtime = serializedModifier.GetStatModifier();
            if (source != null) runtime.Source = source;
            return RemoveStatModifier(groupName, serializedModifier.Stat, runtime, source);
        }

        public virtual bool RemoveAllStatModifiersFromSource(object source)
        {
            bool any = false;
            foreach (MioStatGroup group in _statTable.Values)
                any |= group.RemoveAllModifiersFromSource(source);
            return any;
        }

        public virtual bool RemoveAllStatModifiersFromSource(object source, string groupName, string stat)
        {
            MioStatGroup group = GetGroup(groupName);
            return group != null && group.RemoveAllModifiersFromSource(stat, source);
        }

        /// <summary>Applies every group/modifier in an authored table onto this sheet.</summary>
        public virtual void Apply(MioStatModifierTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            foreach (MioStatModifierGroup group in table.ModifierLists)
                Apply(group);
        }

        public virtual void Apply(MioStatModifierGroup group)
        {
            if (group == null) throw new ArgumentNullException(nameof(group));
            foreach (MioStatModifier mod in group.Modifiers)
                AddStatModifier(group.Group, mod);
        }

        /// <summary>
        /// Applies a table, tagging every modifier with <paramref name="source"/> so the same
        /// table can later be <see cref="Remove(MioStatModifierTable, object)"/>d by source.
        /// </summary>
        public virtual void Apply(MioStatModifierTable table, object source)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            foreach (MioStatModifierGroup group in table.ModifierLists)
                Apply(group, source);
        }

        public virtual void Apply(MioStatModifierGroup group, object source)
        {
            if (group == null) throw new ArgumentNullException(nameof(group));
            foreach (MioStatModifier mod in group.Modifiers)
                AddStatModifier(group.Group, mod, source);
        }

        /// <summary>Removes, by source, the exact modifiers an authored table added.</summary>
        public virtual bool Remove(MioStatModifierTable table, object source)
        {
            if (table == null) return false;
            bool any = false;
            foreach (MioStatModifierGroup group in table.ModifierLists)
                any |= Remove(group, source);
            return any;
        }

        public virtual bool Remove(MioStatModifierGroup group, object source)
        {
            if (group == null) return false;
            bool any = false;
            foreach (MioStatModifier mod in group.Modifiers)
                any |= RemoveStatModifier(group.Group, mod, source);
            return any;
        }

        // =======================================================================================
        // Parent structure — bump _structureVersion ONLY here.
        // =======================================================================================

        /// <summary>Links a parent sheet. No-op for null/self/duplicate parents.</summary>
        public virtual void AddParentTable(MioStatSheet parent)
        {
            if (parent == null || ReferenceEquals(parent, this)) return;
            if (_parentTables.Contains(parent)) return; // duplicates would double-contribute
            _parentTables.Add(parent);
            _structureVersion++;
        }

        public virtual bool RemoveParentTable(MioStatSheet parent)
        {
            if (parent == null) return false;
            if (_parentTables.Remove(parent))
            {
                _structureVersion++;
                return true;
            }
            return false;
        }

        public virtual bool HasParentTable(MioStatSheet parent) => parent != null && _parentTables.Contains(parent);

        // =======================================================================================
        // Life cycle
        // =======================================================================================

        /// <summary>
        /// Clears all content and the cache. NEVER resets _structureVersion or any MioStat version —
        /// counters are monotonic (a reset would let a released-and-repopulated table re-collide with
        /// a child's recorded snapshot and serve stale references).
        ///
        /// Pooling contract: if this sheet is reused as a parent across logical entities while another
        /// sheet still caches it, REUSE the same MioStat/MioStatGroup instances too (they keep their
        /// versions climbing) rather than repopulating freshly-created stats with an identical
        /// modifier multiset.
        /// </summary>
        public virtual void Release()
        {
            _cache.Clear();
            _statTable.Clear();
            _parentTables.Clear();
            _readOnlyStatTable = null;
        }

        // =======================================================================================
        // Cache internals
        // =======================================================================================

        /// <summary>
        /// Captures the query's group names into the shared buffer, deduped + sorted + canonical
        /// (ordinal). Returns the count; groups points at the shared buffer (frame-local use only).
        /// </summary>
        private static int CaptureAndSortGroupNames(IEnumerable<string> groupNames, out string[] groups)
        {
            groups = s_groupBuffer;
            int count = 0;
            if (groupNames != null)
            {
                foreach (string g in groupNames)
                {
                    if (string.IsNullOrEmpty(g)) continue;

                    bool dup = false;
                    for (int i = 0; i < count; i++)
                    {
                        if (string.Equals(groups[i], g, StringComparison.Ordinal)) { dup = true; break; }
                    }
                    if (dup) continue;

                    if (count == groups.Length)
                    {
                        Array.Resize(ref groups, groups.Length * 2);
                        s_groupBuffer = groups;
                    }
                    groups[count++] = g;
                }
            }

            Array.Sort(groups, 0, count, StringComparer.Ordinal);
            return count;
        }

        private static string BuildKey(bool includeParents, string[] groups, int count, string stat)
        {
            StringBuilder sb = s_keyBuilder;
            sb.Length = 0;
            sb.Append(includeParents ? 'P' : 'L');
            sb.Append('|');
            for (int i = 0; i < count; i++)
            {
                if (i > 0) sb.Append(',');
                AppendEscaped(sb, groups[i]);
            }
            sb.Append('|');
            AppendEscaped(sb, stat);
            return sb.ToString();
        }

        private static void AppendEscaped(StringBuilder sb, string s)
        {
            if (s == null) return;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\\' || c == ',' || c == '|') sb.Append('\\');
                sb.Append(c);
            }
        }

        /// <summary>
        /// Single ancestor traversal shared by build and validate. Depth-first pre-order over the
        /// live parent lists, reference-deduped, capped at MaxAncestors — this IS the cycle guard.
        /// </summary>
        private static int WalkAncestors(MioStatSheet node, MioStatSheet[] visited, int start)
        {
            int count = start;
            for (int i = 0; i < node._parentTables.Count && count < MaxAncestors; i++)
            {
                MioStatSheet p = node._parentTables[i];

                bool dup = false;
                for (int j = 0; j < count; j++)
                {
                    if (ReferenceEquals(visited[j], p)) { dup = true; break; }
                }
                if (dup) continue;

                visited[count++] = p;
                if (count >= MaxAncestors) break;
                count = WalkAncestors(p, visited, count);
            }
            return count;
        }

        private static bool s_cycleLogged;
        private static void LogCycle()
        {
            if (s_cycleLogged) return;
            s_cycleLogged = true;
            UnityEngine.Debug.LogWarning("[MioHelper.StatSystem] Parent-table cycle detected (chain deeper than 10). " +
                "Check AddParentTable calls — no sheet may be reachable from itself through its parents.");
        }

        private CacheEntry BuildLeaf(string[] groups, int count, string stat)
        {
            List<StatModifier> mods = new List<StatModifier>();
            CollectLeafMods(groups, count, stat, mods);
            return new CacheEntry
            {
                Modifiers = mods,
                ReadOnly = mods.AsReadOnly(),
                IncludesParents = false,
                SelfGroups = CaptureSelf(groups, count, stat),
                Chain = Array.Empty<ChainLink>(),
            };
        }

        private CacheEntry BuildChain(string[] groups, int count, string stat, int step)
        {
            // Owned copy of the group names: the recursive ancestor reads below re-use the shared
            // s_groupBuffer, which must not be aliased while we still need the names.
            string[] query = new string[count];
            Array.Copy(groups, query, count);

            List<StatModifier> mods = new List<StatModifier>();
            CollectLeafMods(query, count, stat, mods);
            GroupCapture[] selfGroups = CaptureSelf(query, count, stat);

            // Capture every link BEFORE recursing into ancestor reads (which clobber the shared
            // ancestor buffer). Ancestor mods are then collected from the captured table refs.
            int n = WalkAncestors(this, s_walkVisited, 0);
            ChainLink[] links = new ChainLink[n];
            for (int i = 0; i < n; i++)
                links[i] = CaptureLink(s_walkVisited[i], query, count, stat);

            for (int i = 0; i < n; i++)
                mods.AddRange(links[i].Table.GetStatModifiersIncludingParent(query, stat, step + 1));

            return new CacheEntry
            {
                Modifiers = mods,
                ReadOnly = mods.AsReadOnly(),
                IncludesParents = true,
                SelfGroups = selfGroups,
                Chain = links,
            };
        }

        private void CollectLeafMods(string[] groups, int count, string stat, List<StatModifier> into)
        {
            for (int i = 0; i < count; i++)
            {
                MioStatGroup group = GetGroup(groups[i]);
                if (group == null) continue;
                MioStat s = group.GetStat(stat);
                if (s == null) continue;
                into.AddRange(s.RawModifiers);
            }
        }

        private GroupCapture[] CaptureSelf(string[] groups, int count, string stat)
        {
            GroupCapture[] arr = new GroupCapture[count];
            for (int i = 0; i < count; i++)
            {
                string g = groups[i];
                MioStatGroup group = GetGroup(g);
                MioStat s = group?.GetStat(stat);
                arr[i] = new GroupCapture(g, group != null, s != null, s != null ? s.ModifierVersion : 0);
            }
            return arr;
        }

        private static ChainLink CaptureLink(MioStatSheet ancestor, string[] groups, int count, string stat)
        {
            GroupCapture[] caps = new GroupCapture[count];
            for (int i = 0; i < count; i++)
            {
                string g = groups[i];
                MioStatGroup group = ancestor.GetGroup(g);
                MioStat s = group?.GetStat(stat);
                caps[i] = new GroupCapture(g, group != null, s != null, s != null ? s.ModifierVersion : 0);
            }
            return new ChainLink(ancestor, ancestor._structureVersion, caps);
        }

        private bool ValidLeaf(in CacheEntry entry, string stat)
        {
            GroupCapture[] caps = entry.SelfGroups;
            for (int i = 0; i < caps.Length; i++)
            {
                GroupCapture cap = caps[i];
                MioStatGroup group = GetGroup(cap.Group);
                if ((group != null) != cap.GroupPresent) return false;

                MioStat s = group?.GetStat(stat);
                if ((s != null) != cap.StatPresent) return false;
                if (s != null && s.ModifierVersion != cap.StatVersion) return false;
            }
            return true;
        }

        private bool ValidChain(in CacheEntry entry, string stat)
        {
            if (!ValidLeaf(in entry, stat)) return false;

            ChainLink[] chain = entry.Chain;
            if (chain == null) return false;

            int n = WalkAncestors(this, s_walkVisited, 0);
            if (n != chain.Length) return false; // parent added/removed

            for (int i = 0; i < n; i++)
            {
                MioStatSheet live = s_walkVisited[i];
                ChainLink link = chain[i];

                // Reference-exact re-walk — a removed-but-unchanged parent is still a mismatch.
                if (!ReferenceEquals(live, link.Table)) return false;
                if (live._structureVersion != link.StructureVersion) return false;

                GroupCapture[] caps = link.Groups;
                for (int j = 0; j < caps.Length; j++)
                {
                    GroupCapture cap = caps[j];
                    MioStatGroup group = live.GetGroup(cap.Group);
                    if ((group != null) != cap.GroupPresent) return false;

                    MioStat s = group?.GetStat(stat);
                    if ((s != null) != cap.StatPresent) return false;
                    if (s != null && s.ModifierVersion != cap.StatVersion) return false;
                }
            }
            return true;
        }
    }
}
