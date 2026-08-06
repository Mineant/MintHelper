using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Abstract base for selecting methods. Uses the template method pattern:
    /// 1. Get all selectables from the provider
    /// 2. Filter by affiliation, distance, line-of-sight
    /// 3. Select targets in the shape (circle, rect, cone, etc.)
    /// 4. Order by selection method (closest/furthest)
    /// 5. Limit to max targets
    ///
    /// Projects implement <see cref="ISelectableProvider"/> to supply entities.
    /// Subclasses implement <see cref="SelectTargetsInShape"/> for geometric filtering.
    /// </summary>
    public abstract class SelectingMethodBase
    {
        /// <summary>Injected provider of selectable entities.</summary>
        public ISelectableProvider SelectableProvider { get; set; }

        public SelectionInfo GetTargets(Vector3 origin, Vector3 aimPosition, Vector3 aimDirection,
            float maxDistance, float range, int maxTargets,
            BestTargetSelectionMethod selectionMethod, TargetAffiliation affiliation,
            GameObject owner)
        {
            var selectionInfo = new SelectionInfo
            {
                Targets = new List<ISelectable>(),
                SelectionCenter = origin,
                SelectionDirection = aimDirection
            };

            if (!IsValidInput(origin, aimPosition, aimDirection))
                return selectionInfo;

            var selectables = GetAllSelectables();
            var validTargets = FilterValidSelectables(selectables, origin, maxDistance, affiliation, owner);

            validTargets = SelectTargetsInShape(validTargets, origin, aimPosition, aimDirection, maxDistance, range);

            if (validTargets == null || !validTargets.Any())
                return selectionInfo;

            validTargets = OrderTargetsBySelectionMethod(validTargets, origin, selectionMethod);
            validTargets = LimitTargets(validTargets, maxTargets);

            selectionInfo.Targets = validTargets.ToList();
            return selectionInfo;
        }

        protected virtual bool IsValidInput(Vector3 origin, Vector3 aimPosition, Vector3 aimDirection) => true;

        protected IEnumerable<ISelectable> GetAllSelectables()
        {
            return SelectableProvider?.GetAllSelectables() ?? Enumerable.Empty<ISelectable>();
        }

        /// <summary>Subclasses implement this to filter by shape (circle, rect, cone, etc.).</summary>
        protected abstract IEnumerable<ISelectable> SelectTargetsInShape(
            IEnumerable<ISelectable> validTargets,
            Vector3 origin, Vector3 aimPosition, Vector3 aimDirection,
            float maxDistance, float range);

        public static IEnumerable<ISelectable> FilterValidSelectables(
            IEnumerable<ISelectable> selectables, Vector3 origin,
            float maxDistance, TargetAffiliation affiliation, GameObject owner)
        {
            var ownerTeam = owner?.GetComponent<ITeamMember>();

            foreach (var target in selectables)
            {
                if (!target.IsValidTarget) continue;
                if (target.Transform == null) continue;

                if (maxDistance > 0 && Vector3.Distance(origin, target.Transform.position) > maxDistance)
                    continue;

                if (!IsValidAffiliation(target, affiliation, owner, ownerTeam))
                    continue;

                yield return target;
            }
        }

        public static bool IsValidAffiliation(ISelectable target, TargetAffiliation affiliation,
            GameObject owner, ITeamMember ownerTeam)
        {
            if (affiliation == TargetAffiliation.All) return true;

            var targetTeam = target.Transform?.GetComponent<ITeamMember>();

            switch (affiliation)
            {
                case TargetAffiliation.Self:
                    return target.Transform?.gameObject == owner;
                case TargetAffiliation.Ally:
                    return targetTeam != null && ownerTeam != null
                        && targetTeam.TeamId == ownerTeam.TeamId
                        && target.Transform?.gameObject != owner;
                case TargetAffiliation.AllyIncludeSelf:
                    return targetTeam != null && ownerTeam != null
                        && targetTeam.TeamId == ownerTeam.TeamId;
                case TargetAffiliation.Enemy:
                    return targetTeam == null || ownerTeam == null
                        || targetTeam.TeamId != ownerTeam.TeamId;
                default:
                    return false;
            }
        }

        public static IEnumerable<ISelectable> OrderTargetsBySelectionMethod(
            IEnumerable<ISelectable> targets, Vector3 origin, BestTargetSelectionMethod method)
        {
            if (targets == null || !targets.Any()) return targets;

            return method switch
            {
                BestTargetSelectionMethod.ClosestDistance =>
                    targets.OrderBy(t => Vector3.Distance(origin, t.Transform.position)),
                BestTargetSelectionMethod.FurthestDistance =>
                    targets.OrderByDescending(t => Vector3.Distance(origin, t.Transform.position)),
                _ => targets
            };
        }

        public static IEnumerable<ISelectable> LimitTargets(
            IEnumerable<ISelectable> targets, int maxTargets)
        {
            if (maxTargets <= 0 || targets == null) return targets ?? Enumerable.Empty<ISelectable>();
            return targets.Take(maxTargets);
        }
    }
}
