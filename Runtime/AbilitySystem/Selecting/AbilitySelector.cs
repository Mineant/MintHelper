using System;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Selecting shape types. Geometric methods (circle, rect, cone) are pure math
    /// and provided as samples. Projects can add custom selecting types.
    /// </summary>
    public enum SelectingType
    {
        NoSelection = 0,
        WholeRange = 1,
        Circle = 2,
        Rect = 3,
        Cone = 4,
        Single = 6,
    }

    public enum BestTargetSelectionMethod { ClosestDistance, FurthestDistance }

    public enum TargetAffiliation { All, Self, Ally, AllyIncludeSelf, Enemy }

    /// <summary>
    /// Design-time configuration for how an ability selects targets.
    /// </summary>
    [Serializable]
    public class AbilitySelector : AbilityComponent<RuntimeAbilitySelector>
    {
        public float Distance;
        public float Range;
        public int MaxTargets;
        public bool NeedClearLineOfSight;
        public bool VisualSelector;
        public SelectingType SelectingType;
        public BestTargetSelectionMethod BestTargetSelectionMethod;
        public TargetAffiliation TargetAffiliation;

        protected override RuntimeAbilitySelector GetNewRuntimeAbilityComponent()
        {
            return new RuntimeAbilitySelector();
        }
    }
}
