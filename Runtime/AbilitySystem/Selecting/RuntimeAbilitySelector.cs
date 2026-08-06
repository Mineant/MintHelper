using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Runtime controller for target selection. Uses a state machine to manage
    /// manual selection (input-driven) and auto selection (AI-driven).
    /// </summary>
    [Serializable]
    public class RuntimeAbilitySelector : RuntimeAbilityComponent<AbilitySelector>
    {
        private enum SelectionState { None, Selecting }

        private StateMachine _selectionSM;
        private ISelectingInput _input;
        private ISelectingVisual _visual;
        private SelectingMethodBase _selectingMethod;
        private Action<bool, SelectionInfo> _currentCallback;

        public override async UniTask InitAsync(AbilitySelector ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
            await base.InitAsync(ownerAbilityComponent, ownerRuntimeAbility);

            _selectionSM = new StateMachine();
            _selectionSM.AddState((int)SelectionState.None, null);
            _selectionSM.AddState((int)SelectionState.Selecting, SelectingOnEnter, SelectingOnUpdate, SelectingOnExit);

            // Get input and visual from factory if available, otherwise use defaults
            var factory = OwnerGameObject?.GetComponent<IAbilitySelectionFactory>();
            if (factory != null)
            {
                _input = factory.CreateInput();
                _visual = factory.CreateVisual(OwnerGameObject);
            }

            _selectingMethod = GetSelectingMethod(ownerAbilityComponent.SelectingType);
            if (_selectingMethod != null)
            {
                // Get selectable provider from the game object or its hierarchy
                _selectingMethod.SelectableProvider = OwnerGameObject?.GetComponent<ISelectableProvider>();
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            _selectionSM?.UpdateState(deltaTime);
        }

        #region Auto Selection

        public virtual void StartAutoSelectionProcess(Action<bool, SelectionInfo> onSelectionFinish)
        {
            if (_selectingMethod == null)
            {
                onSelectionFinish?.Invoke(false, null);
                return;
            }

            Vector3 origin = OwnerGameObject != null ? OwnerGameObject.transform.position : Vector3.zero;
            var selectables = _selectingMethod.GetAllSelectables();
            var validTargets = SelectingMethodBase.FilterValidSelectables(
                selectables, origin,
                OwnerAbilityComponent.Distance,
                OwnerAbilityComponent.TargetAffiliation,
                OwnerGameObject);

            if (!validTargets.Any())
            {
                onSelectionFinish?.Invoke(false, null);
                return;
            }

            var firstTarget = validTargets.First();
            Vector3 aimPosition = firstTarget.Transform.position;
            Vector3 aimDirection = (aimPosition - origin).normalized;

            var selectionInfo = _selectingMethod.GetTargets(
                origin, aimPosition, aimDirection,
                OwnerAbilityComponent.Distance, OwnerAbilityComponent.Range,
                OwnerAbilityComponent.MaxTargets,
                OwnerAbilityComponent.BestTargetSelectionMethod,
                OwnerAbilityComponent.TargetAffiliation,
                OwnerGameObject);

            bool success = selectionInfo?.Targets != null && selectionInfo.Targets.Any();
            onSelectionFinish?.Invoke(success, selectionInfo);
        }

        public virtual SelectionInfo GetAutoTargets()
        {
            if (_selectingMethod == null) return new SelectionInfo { Targets = new List<ISelectable>() };

            Vector3 origin = OwnerGameObject != null ? OwnerGameObject.transform.position : Vector3.zero;
            Vector3 aimDirection = OwnerGameObject != null ? OwnerGameObject.transform.forward : Vector3.forward;

            return _selectingMethod.GetTargets(
                origin, origin, aimDirection,
                OwnerAbilityComponent.Distance, OwnerAbilityComponent.Range,
                OwnerAbilityComponent.MaxTargets,
                OwnerAbilityComponent.BestTargetSelectionMethod,
                OwnerAbilityComponent.TargetAffiliation,
                OwnerGameObject);
        }

        #endregion

        #region Manual Selection

        public virtual void StartManualSelectionProcess(Action<bool, SelectionInfo> onSelectionFinish)
        {
            if (_selectionSM.CurrentState == (int)SelectionState.Selecting) return;
            _currentCallback = onSelectionFinish;
            _selectionSM.ChangeState((int)SelectionState.Selecting);
        }

        private Vector3 _currentSelectionPoint;

        private void SelectingOnEnter(StateParam param)
        {
            _currentSelectionPoint = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        }

        private void SelectingOnUpdate(float deltaTime)
        {
            if (_input == null) return;

            _currentSelectionPoint = _input.GetTargetingPosition();

            Vector3 origin = OwnerGameObject != null ? OwnerGameObject.transform.position : Vector3.zero;
            float maxRange = OwnerAbilityComponent.Distance;

            // Clamp to max range
            Vector3 toSelection = _currentSelectionPoint - origin;
            if (toSelection.magnitude > maxRange)
                _currentSelectionPoint = origin + toSelection.normalized * maxRange;

            Vector3 direction = _currentSelectionPoint - origin;

            var currentSelection = _selectingMethod?.GetTargets(
                origin, _currentSelectionPoint, direction,
                maxRange, OwnerAbilityComponent.Range,
                OwnerAbilityComponent.MaxTargets,
                OwnerAbilityComponent.BestTargetSelectionMethod,
                OwnerAbilityComponent.TargetAffiliation,
                OwnerGameObject);

            if (OwnerAbilityComponent.VisualSelector && _visual != null)
                _visual.ShowTargeting(currentSelection, this, _currentSelectionPoint, direction);

            if (_input.GetConfirmInput())
            {
                _currentCallback?.Invoke(true, currentSelection);
                _selectionSM.ChangeState((int)SelectionState.None);
            }
            else if (_input.GetCancelInput())
            {
                _currentCallback?.Invoke(false, null);
                _selectionSM.ChangeState((int)SelectionState.None);
            }
        }

        private void SelectingOnExit()
        {
            _currentSelectionPoint = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            _visual?.HideTargeting();
            _currentCallback = null;
        }

        #endregion

        #region Selecting Method Factory

        /// <summary>
        /// Return a selecting method for the given type. Override or extend to add
        /// project-specific selecting methods.
        /// </summary>
        public virtual SelectingMethodBase GetSelectingMethod(SelectingType type)
        {
            return type switch
            {
                SelectingType.NoSelection => null,
                SelectingType.WholeRange => new WholeRangeSelectingMethod(),
                SelectingType.Circle => new CircleSelectingMethod(),
                SelectingType.Rect => new RectSelectingMethod(),
                SelectingType.Cone => new ConeSelectingMethod(),
                SelectingType.Single => new SingleTargetSelectingMethod(),
                _ => null
            };
        }

        #endregion
    }

    #region State Machine (simple embedded FSM)

    /// <summary>
    /// Simple embedded finite state machine used by RuntimeAbilitySelector.
    /// </summary>
    internal class StateMachine
    {
        private Dictionary<int, StateDef> _states = new();
        public int CurrentState { get; private set; }

        public void AddState(int id, Action<StateParam> onEnter, Action<float> onUpdate, Action onExit)
        {
            _states[id] = new StateDef { OnEnter = onEnter, OnUpdate = onUpdate, OnExit = onExit };
        }

        public void ChangeState(int newState)
        {
            if (_states.TryGetValue(CurrentState, out var oldDef))
                oldDef.OnExit?.Invoke();

            CurrentState = newState;

            if (_states.TryGetValue(newState, out var newDef))
                newDef.OnEnter?.Invoke(new StateParam());
        }

        public void UpdateState(float deltaTime)
        {
            if (_states.TryGetValue(CurrentState, out var def))
                def.OnUpdate?.Invoke(deltaTime);
        }

        private struct StateDef
        {
            public Action<StateParam> OnEnter;
            public Action<float> OnUpdate;
            public Action OnExit;
        }
    }

    public class StateParam { }

    #endregion
}
