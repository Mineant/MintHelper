using System.Collections.Generic;
using MioHelper.StatSystem;
using UnityEngine;

namespace MioHelper.Samples.StatSystem
{
    /// <summary>
    /// Self-bootstrapping demo of the Stat System module (no scene assets needed).
    /// Shows: base + flat/percent/mult stacking, parent-table inheritance, source-based removal,
    /// and the live in-place StatModifier field write (no cache invalidation required).
    /// Attach to any GameObject, or read the [StatSystem] lines in the console.
    /// </summary>
    public class SampleStatSystemDemo : MonoBehaviour
    {
        void Start()
        {
            MioStatSheet player = new MioStatSheet();

            // ---- authoring DTOs: a base-stat table (no source) and a weapon table (source "weapon") ----
            MioStatModifierTable baseTable = new MioStatModifierTable
            {
                ModifierLists = new List<MioStatModifierGroup>
                {
                    new MioStatModifierGroup
                    {
                        Group = "Character",
                        Modifiers = new List<MioStatModifier>
                        {
                            new MioStatModifier { Stat = "ATTACK", Value = 20f, Type = MioStatModType.Flat },
                        },
                    },
                },
            };

            MioStatModifierTable weaponTable = new MioStatModifierTable
            {
                ModifierLists = new List<MioStatModifierGroup>
                {
                    new MioStatModifierGroup
                    {
                        Group = "Character",
                        Modifiers = new List<MioStatModifier>
                        {
                            new MioStatModifier { Stat = "ATTACK", Value = 10f, Type = MioStatModType.Flat },
                            new MioStatModifier { Stat = "ATTACK", Value = 0.2f, Type = MioStatModType.PercentAdd },
                            new MioStatModifier { Stat = "ATTACK", Value = 0.1f, Type = MioStatModType.Mult },
                        },
                    },
                },
            };

            object weapon = new object();

            player.Apply(baseTable);
            Print("base (100 + 20 flat)", Query(player, 100f), 120f);

            // ---- parent-table inheritance ----
            MioStatSheet team = new MioStatSheet();
            team.Apply(new MioStatModifierTable
            {
                ModifierLists = new List<MioStatModifierGroup>
                {
                    new MioStatModifierGroup
                    {
                        Group = "Character",
                        Modifiers = new List<MioStatModifier>
                        {
                            new MioStatModifier { Stat = "ATTACK", Value = 0.5f, Type = MioStatModType.PercentAdd },
                        },
                    },
                },
            });
            player.AddParentTable(team);
            Print("team parent (+50% pct) but no weapon yet", Query(player, 100f), 180f);

            // ---- apply the weapon, then query (leaf + parent-inclusive) ----
            player.Apply(weaponTable, weapon);
            Print("leaf, weapon on (flat+10, +20%, *1.1)", LeafQuery(player, 100f), 171.6f);
            Print("leaf + team parent (pct sums to +70%)", Query(player, 100f), 243.1f);

            // ---- source-based removal ----
            player.Remove(weaponTable, weapon);
            Print("weapon removed by source (only team +50% remains)", Query(player, 100f), 180f);

            // ---- live in-place field write: no invalidation, next read reflects it ----
            player.Apply(weaponTable, weapon);
            StatModifier mult = player.GetStatModifiersIncludingParent(new[] { "Character" }, "ATTACK")
                                        .LastOrNull(m => m.Type == MioStatModType.Mult);
            Print("weapon re-applied", Query(player, 100f), 243.1f);

            if (mult != null)
            {
                mult.Value = 0.5f; // mutating the shared reference in place
                Print("mult.Value bumped 1.1 -> 1.5 (live field, no invalidation)", Query(player, 100f), 331.5f);
                mult.Value = 0.1f;
            }

            Print("leaf + team again after restoring", Query(player, 100f), 243.1f);

            player.Release();
            Print("after Release() (sheet empty)", Query(player, 100f), 100f);
        }

        static float Query(MioStatSheet sheet, float baseValue) =>
            sheet.GetTotalStatValue("ATTACK", new[] { "Character" }, true, baseValue);

        static float LeafQuery(MioStatSheet sheet, float baseValue) =>
            sheet.GetTotalStatValue("ATTACK", new[] { "Character" }, false, baseValue);

        static void Print(string label, float actual, float expected)
        {
            bool pass = Mathf.Abs(actual - expected) < 0.001f;
            print($"[StatSystem] {(pass ? "PASS" : "FAIL")} {label}: {actual.ToString("0.###")} (expected {expected.ToString("0.###")})");
        }
    }

    internal static class StatSystemSampleExtensions
    {
        public static T LastOrNull<T>(this IEnumerable<T> source, System.Func<T, bool> predicate) where T : class
        {
            T found = null;
            if (source != null)
            {
                foreach (T item in source)
                {
                    if (predicate(item)) found = item;
                }
            }
            return found;
        }
    }
}
