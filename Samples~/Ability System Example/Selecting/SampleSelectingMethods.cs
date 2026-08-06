using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MioHelper.AbilitySystem.Samples
{
    /// <summary>
    /// Selects all valid targets (no shape filtering).
    /// </summary>
    public class WholeRangeSelectingMethod : SelectingMethodBase
    {
        protected override IEnumerable<ISelectable> SelectTargetsInShape(
            IEnumerable<ISelectable> validTargets,
            Vector3 origin, Vector3 aimPosition, Vector3 aimDirection,
            float maxDistance, float range)
        {
            return validTargets;
        }
    }

    /// <summary>
    /// Selects targets within a circular area.
    /// </summary>
    public class CircleSelectingMethod : SelectingMethodBase
    {
        protected override bool IsValidInput(Vector3 origin, Vector3 aimPosition, Vector3 aimDirection)
        {
            return aimPosition.x != Mathf.NegativeInfinity;
        }

        protected override IEnumerable<ISelectable> SelectTargetsInShape(
            IEnumerable<ISelectable> validTargets,
            Vector3 origin, Vector3 aimPosition, Vector3 aimDirection,
            float maxDistance, float range)
        {
            Vector3 circleCenter = aimPosition;
            if (Vector3.Distance(origin, circleCenter) > maxDistance)
            {
                Vector3 dirToPoint = (circleCenter - origin).normalized;
                circleCenter = origin + dirToPoint * maxDistance;
            }

            return validTargets.Where(target =>
                Vector3.Distance(circleCenter, target.Transform.position) <= range);
        }
    }

    /// <summary>
    /// Selects targets within a rectangular area oriented along the aim direction.
    /// </summary>
    public class RectSelectingMethod : SelectingMethodBase
    {
        protected override bool IsValidInput(Vector3 origin, Vector3 aimPosition, Vector3 aimDirection)
        {
            return aimDirection != Vector3.zero;
        }

        protected override IEnumerable<ISelectable> SelectTargetsInShape(
            IEnumerable<ISelectable> validTargets,
            Vector3 origin, Vector3 aimPosition, Vector3 aimDirection,
            float maxDistance, float range)
        {
            Vector3 forward = aimDirection.normalized;
            Vector3 right = Vector3.Cross(forward, Vector3.forward).normalized;
            float width = range;
            float length = maxDistance;

            return validTargets.Where(target =>
            {
                Vector3 targetPos = target.Transform.position - origin;
                float forwardProj = Vector3.Dot(targetPos, forward);
                float rightProj = Vector3.Dot(targetPos, right);
                return forwardProj >= 0 && forwardProj <= length && Mathf.Abs(rightProj) <= width / 2;
            });
        }
    }

    /// <summary>
    /// Selects targets within a cone-shaped area.
    /// </summary>
    public class ConeSelectingMethod : SelectingMethodBase
    {
        protected override IEnumerable<ISelectable> SelectTargetsInShape(
            IEnumerable<ISelectable> validTargets,
            Vector3 origin, Vector3 aimPosition, Vector3 aimDirection,
            float maxDistance, float range)
        {
            if (aimDirection == Vector3.zero) return validTargets;

            Vector3 forward = aimDirection.normalized;
            float coneAngle = range; // Range is used as cone angle in degrees

            return validTargets.Where(target =>
            {
                Vector3 dirToTarget = (target.Transform.position - origin).normalized;
                float angle = Vector3.Angle(forward, dirToTarget);
                return angle <= coneAngle / 2;
            });
        }
    }

    /// <summary>
    /// Selects the closest single target to the aim position.
    /// </summary>
    public class SingleTargetSelectingMethod : SelectingMethodBase
    {
        protected override bool IsValidInput(Vector3 origin, Vector3 aimPosition, Vector3 aimDirection)
        {
            return aimPosition.x != Mathf.NegativeInfinity;
        }

        protected override IEnumerable<ISelectable> SelectTargetsInShape(
            IEnumerable<ISelectable> validTargets,
            Vector3 origin, Vector3 aimPosition, Vector3 aimDirection,
            float maxDistance, float range)
        {
            float selectionThreshold = range;
            var closest = validTargets
                .Where(t => Vector3.Distance(aimPosition, t.Transform.position) <= selectionThreshold)
                .OrderBy(t => Vector3.Distance(aimPosition, t.Transform.position))
                .FirstOrDefault();

            return closest != null ? new[] { closest } : Enumerable.Empty<ISelectable>();
        }
    }
}
