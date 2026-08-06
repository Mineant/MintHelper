using System;
using System.Collections.Generic;

namespace MioHelper.StatSystem
{
    /// <summary>
    /// Static stat-math core. Pure function — no scene dependency, no singleton.
    ///
    /// <see cref="CalculateFinalValue"/> runs on EVERY read of a sheet's total value; baseValue is
    /// injected per call, so the final value is never cached. The fold re-reads StatModifier
    /// fields live and sorts a copy, which is what keeps in-place field writes visible.
    /// Single-threaded (Unity main thread): s_foldBuffer is a shared growable buffer.
    /// </summary>
    public static class StatSystem
    {
        // Growable static buffer — no fixed 256 cap, no per-call array. Grows by doubling on the rare
        // oversized fold, then stays rented for the process. NEVER used from multiple threads.
        private static StatModifier[] s_foldBuffer = new StatModifier[16];

        private sealed class StatModifierComparer : IComparer<StatModifier>
        {
            public static readonly StatModifierComparer Instance = new StatModifierComparer();
            public int Compare(StatModifier a, StatModifier b) => a.Order.CompareTo(b.Order);
        }

        /// <summary>
        /// Computes the stacked total: Flat mods add to the running value, PercentAdd mods accumulate
        /// and are applied once to the whole result, Mult mods multiply the running value.
        /// Ordering is by StatModifier.Order (defaults to the type's value).
        /// </summary>
        public static float CalculateFinalValue(IEnumerable<StatModifier> modifiers, float baseValue)
        {
            int count = 0;
            if (modifiers != null)
            {
                foreach (StatModifier m in modifiers)
                {
                    if (m == null) continue;
                    if (count == s_foldBuffer.Length)
                        Array.Resize(ref s_foldBuffer, s_foldBuffer.Length * 2);
                    s_foldBuffer[count++] = m;
                }
            }

            if (count == 0) return baseValue;

            Array.Sort(s_foldBuffer, 0, count, StatModifierComparer.Instance);

            float final = baseValue;
            float sumPercentAdd = 0f;
            for (int i = 0; i < count; i++)
            {
                StatModifier mod = s_foldBuffer[i];
                switch (mod.Type)
                {
                    case MioStatModType.Flat: final += mod.Value; break;
                    case MioStatModType.PercentAdd: sumPercentAdd += mod.Value; break;
                    case MioStatModType.Mult: final *= 1f + mod.Value; break;
                }
            }

            if (sumPercentAdd != 0f) final *= 1f + sumPercentAdd;
            return final;
        }
    }
}
