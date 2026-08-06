Stat System Example
===================

Self-bootstrapping demo of the Stat System module (`MioHelper.StatSystem`). No scene assets needed.

What it shows
-------------
- Authoring DTOs (`MioStatModifier`, `MioStatModifierGroup`, `MioStatModifierTable`) applied onto a
  runtime `MioStatSheet`.
- Flat / PercentAdd / Mult stacking and how `GetTotalStatValue(stat, groupNames, lookupParentTables, baseValue)`
  injects `baseValue` per call (never cached).
- Parent-table inheritance: a sheet's chain of `AddParentTable` ancestors contributes to the
  parent-inclusive query. The versioned cache detects parent changes by re-walking the live chain.
- Source-based removal: `Remove(table, source)` undoes exactly what `Apply(table, source)` added.
- Live in-place `StatModifier.Value` writes: the fold re-reads public fields every call, so the next
  read reflects the change with no cache invalidation.

How to run
----------
Attach `SampleStatSystemDemo` to any GameObject in a scene and enter Play mode, or read the
[StatSystem] PASS/FAIL lines in the console.

Notes for consumers
-------------------
- Never reuse a sheet as a parent for a new entity while another sheet still caches it, unless you
  also release the child. Reuse the same `MioStat`/`MioStatGroup` instances across `Release()`
  (instead of repopulating fresh stats) so their monotonic version counters keep climbing.
- Remove modifiers with a source to disambiguate modifiers that are equal on value/type/order.
