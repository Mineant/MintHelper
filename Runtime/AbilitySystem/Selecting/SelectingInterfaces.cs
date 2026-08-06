using System;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// A thing that can be selected as an ability target.
    /// Projects implement this on characters, monsters, destructibles, etc.
    /// </summary>
    public interface ISelectable
    {
        bool IsValidTarget { get; }
        Transform Transform { get; }
        Vector3 GetHitPosition();
    }

    /// <summary>
    /// Team membership for target affiliation filtering (ally, enemy, self).
    /// Projects implement this on their entities.
    /// </summary>
    public interface ITeamMember
    {
        int TeamId { get; }
    }

    /// <summary>
    /// Provides the pool of selectable entities. Projects implement this to
    /// supply their entity registry to the selecting system.
    /// </summary>
    public interface ISelectableProvider
    {
        IEnumerable<ISelectable> GetAllSelectables();
    }

    /// <summary>
    /// Input source for manual target selection (mouse, controller, touch).
    /// Projects implement this to provide their input system.
    /// </summary>
    public interface ISelectingInput
    {
        Vector2 GetTargetingPosition();
        bool GetConfirmInput();
        bool GetCancelInput();
    }

    /// <summary>
    /// Visual indicator for target selection (gizmos, decals, UI).
    /// Projects implement this to provide their visual style.
    /// </summary>
    public interface ISelectingVisual
    {
        void ShowTargeting(SelectionInfo selectionInfo, RuntimeAbilitySelector parameters, Vector3 aimPosition, Vector3 aimDirection);
        void HideTargeting();
    }

    /// <summary>
    /// Factory for creating input and visual providers for the selecting system.
    /// Projects implement this to inject their platform-specific implementations.
    /// </summary>
    public interface IAbilitySelectionFactory
    {
        ISelectingInput CreateInput();
        ISelectingVisual CreateVisual(GameObject owner);
    }

    /// <summary>
    /// Data class returned by the selecting system after resolving targets.
    /// </summary>
    [Serializable]
    public class SelectionInfo
    {
        public List<ISelectable> Targets;
        public Vector3 SelectionCenter;
        public Vector3 SelectionDirection;
    }
}
