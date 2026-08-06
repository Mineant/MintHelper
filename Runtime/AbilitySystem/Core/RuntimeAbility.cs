using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using MioHelper;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Runtime instance of an <see cref="Ability"/>. Created when an ability is added to a
    /// <see cref="CharacterAbilityModule"/>. Manages the lifecycle: initialization of runtime
    /// components, selection, execution via coroutine, and cleanup.
    ///
    /// Passive abilities: triggered by game events via their trigger components.
    /// Active abilities: manually invoked via UseAbility() or UseAbilityInSlot().
    /// </summary>
    [Serializable]
    public class RuntimeAbility
    {
        #region Fields

        public string ID;
        public CharacterAbilityModule OwnerAbilityComponent;
        public Ability OwnerAbility;

        [SerializeField] private RuntimeAbilitySelector _runtimeSelector;
        [SerializeField] private RuntimeAbilityCooldownComponent _cooldownComponent;
        [SerializeField] private RuntimeAbilityTrigger[] _runtimeTriggers = Array.Empty<RuntimeAbilityTrigger>();
        [SerializeField] private RuntimeAbilityCondition[] _runtimeConditions = Array.Empty<RuntimeAbilityCondition>();
        [SerializeField] private RuntimeAbilityEffect[] _runtimeEffects = Array.Empty<RuntimeAbilityEffect>();
        [SerializeField] private HashSet<string> _statGroups = new();
        [SerializeField] private bool _isAbilityRunning;

        private List<RuntimeAbilityComponent> _runtimeComponents = new();
        private List<Coroutine> _runningCoroutines = new();
        private IAbilityEventBus _eventBus;

        #endregion

        #region Properties

        public bool IsAbilityRunning => _isAbilityRunning;
        public IAbilityOwner Owner => OwnerAbilityComponent?.Owner;

        #endregion

        #region Initialization & Cleanup

        /// <summary>
        /// Initialize the runtime ability from a design-time Ability config.
        /// Creates all runtime components (triggers, conditions, effects, cooldown, selector).
        /// </summary>
        public async UniTask InitializeAsync(CharacterAbilityModule ownerAbilityComponent, Ability ownerAbility)
        {
            OwnerAbilityComponent = ownerAbilityComponent;
            OwnerAbility = ownerAbility;
            ID = Guid.NewGuid().ToString();
            _statGroups = new HashSet<string>(ownerAbility.StatGroups ?? Array.Empty<string>());
            _eventBus = ownerAbilityComponent?.GetComponent<IAbilityEventBus>();

            await InitializeAbilityComponentsAsync(ownerAbility);

            // Active abilities should not have passive triggers
            if (ownerAbility.Type == AbilityType.Active && _runtimeTriggers.Length > 0)
            {
                Debug.LogError($"[AbilitySystem] Active ability '{ownerAbility.Name}' has triggers — triggers are for passive abilities only. Removing triggers.");
                _runtimeComponents.RemoveAll(x => _runtimeTriggers.Contains(x));
                _runtimeTriggers = Array.Empty<RuntimeAbilityTrigger>();
            }
        }

        private async UniTask InitializeAbilityComponentsAsync(Ability ownerAbility)
        {
            _runtimeComponents = new List<RuntimeAbilityComponent>();

            // Triggers
            var triggerList = new List<RuntimeAbilityTrigger>();
            if (ownerAbility.Triggers != null)
            {
                foreach (var trigger in ownerAbility.Triggers)
                    triggerList.Add(trigger.CreateRuntimeAbilityComponent(this));
            }
            _runtimeTriggers = triggerList.ToArray();
            _runtimeComponents.AddRange(_runtimeTriggers);

            // Conditions
            var condList = new List<RuntimeAbilityCondition>();
            if (ownerAbility.Conditions != null)
            {
                foreach (var condition in ownerAbility.Conditions)
                    condList.Add(condition.CreateRuntimeAbilityComponent(this));
            }
            _runtimeConditions = condList.ToArray();
            _runtimeComponents.AddRange(_runtimeConditions);

            // Effects
            var effectList = new List<RuntimeAbilityEffect>();
            if (ownerAbility.Effects != null)
            {
                foreach (var effect in ownerAbility.Effects)
                {
                    var runtimeEffect = await effect.CreateRuntimeAbilityComponentAsync(this);
                    effectList.Add(runtimeEffect);
                }
            }
            _runtimeEffects = effectList.ToArray();
            _runtimeComponents.AddRange(_runtimeEffects);

            // Cooldown
            _cooldownComponent = new RuntimeAbilityCooldownComponent();
            await _cooldownComponent.InitAsync(null, this);
            _runtimeComponents.Add(_cooldownComponent);

            // Selector
            if (ownerAbility.Selector != null)
            {
                _runtimeSelector = await ownerAbility.Selector.CreateRuntimeAbilityComponentAsync(this);
                _runtimeComponents.Add(_runtimeSelector);
            }

            // Finish initialization
            foreach (var component in _runtimeComponents)
                component.OnFinishAllRuntimeAbilityComponentsInit();
        }

        public void Release()
        {
            StopAbilityEffects();
            foreach (var component in _runtimeComponents)
                component.Release();
        }

        #endregion

        #region Stat Groups

        public HashSet<string> GetStatGroups() => _statGroups;

        public void AddStatGroup(string groupName) => _statGroups.Add(groupName);

        public void RemoveStatGroup(string groupName)
        {
            if (!_statGroups.Remove(groupName))
                Debug.LogWarning($"[AbilitySystem] Stat group '{groupName}' does not exist on this ability.");
        }

        #endregion

        #region Update

        public void OnUpdate(float deltaTime)
        {
            foreach (var component in _runtimeComponents)
                component.OnUpdate(deltaTime);
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            foreach (var component in _runtimeComponents)
                component.OnFixedUpdate(fixedDeltaTime);
        }

        #endregion

        #region Selection

        public void StartSelectionProcess(Action<bool, SelectionInfo> onSelectionFinish = null)
        {
            if (_runtimeSelector == null) return;
            if (!_cooldownComponent.CanUseAbility()) return;
            _runtimeSelector.StartManualSelectionProcess(onSelectionFinish ?? DefaultSelectionCallback);
        }

        public void StartAutoSelectionProcess(Action<bool, SelectionInfo> onSelectionFinish = null)
        {
            if (_runtimeSelector == null) return;
            if (!_cooldownComponent.CanUseAbility()) return;
            _runtimeSelector.StartAutoSelectionProcess(onSelectionFinish ?? DefaultSelectionCallback);
        }

        private void DefaultSelectionCallback(bool success, SelectionInfo info)
        {
            if (!success) return;
            UseAbility(new RuntimeAbilityTriggerArgs
            {
                Targets = info.Targets,
                AimPosition = info.SelectionCenter,
                AimDirection = info.SelectionDirection
            });
        }

        #endregion

        #region Execution

        /// <summary>Use the ability with auto-selected targets.</summary>
        public void UseAbility()
        {
            if (_runtimeSelector == null)
            {
                Debug.LogWarning($"[AbilitySystem] Ability '{ID}' has no selector.");
                return;
            }
            var info = _runtimeSelector.GetAutoTargets();
            UseAbility(new RuntimeAbilityTriggerArgs
            {
                Targets = info.Targets,
                AimPosition = info.SelectionCenter,
                AimDirection = info.SelectionDirection
            });
        }

        /// <summary>Use the ability with explicit trigger args.</summary>
        public void UseAbility(RuntimeAbilityTriggerArgs abilityArgs, bool ignoreCondition = false)
        {
            if (!ignoreCondition)
            {
                if (!_cooldownComponent.CanUseAbility()) return;
                if (!CheckCondition(abilityArgs)) return;
            }

            if (IsAbilityRunning)
            {
                Debug.LogWarning($"[AbilitySystem] Ability '{ID}' is already running.");
                return;
            }

            OwnerAbilityComponent.StartCoroutine(UsingAbility(abilityArgs));
        }

        private IEnumerator UsingAbility(RuntimeAbilityTriggerArgs triggerArgs)
        {
            _isAbilityRunning = true;

            float attackSpeed = 1f; // Default — override via IAbilityOwner if available

            RuntimeAbilityEffectArgs effectArgs = new RuntimeAbilityEffectArgs();
            effectArgs.Init(triggerArgs);
            effectArgs.AttackSpeed = attackSpeed;
            effectArgs.Context = new RuntimeAbilityContext();

            // Start all effects
            foreach (var effect in _runtimeEffects)
            {
                if (effect == null) continue;
                _runningCoroutines.Add(OwnerAbilityComponent.StartCoroutine(effect.ExecuteCoroutine(effectArgs)));
            }

            // Notify listeners
            OwnerAbilityComponent.NotifyAbilityActivated(this);
            _eventBus?.OnAbilityActivated(OwnerAbilityComponent, this);

            // Start cooldown immediately
            _cooldownComponent.StartCooldown();

            // Wait for all effects to finish
            foreach (var effect in _runtimeEffects)
            {
                if (effect == null) continue;
                while (!effect.IsExecutionFinished())
                    yield return null;
            }

            foreach (var effect in _runtimeEffects)
            {
                if (effect == null) continue;
                effect.OnAbilityFinished();
            }

            _runningCoroutines.Clear();
            _isAbilityRunning = false;
            OnAbilityComplete();
        }

        public void CancelAbility()
        {
            _isAbilityRunning = false;
            StopAbilityEffects();
            OwnerAbilityComponent?.OnAbilityCancel(this);
        }

        private void StopAbilityEffects()
        {
            foreach (var effect in _runtimeEffects)
                effect?.StopExecution();

            foreach (var coroutine in _runningCoroutines)
            {
                if (coroutine != null)
                    OwnerAbilityComponent.StopCoroutine(coroutine);
            }
            _runningCoroutines.Clear();
        }

        #endregion

        #region Conditions

        public bool CheckCondition(RuntimeAbilityTriggerArgs abilityArgs)
        {
            foreach (var condition in _runtimeConditions)
            {
                if (!condition.CheckCondition(abilityArgs))
                    return false;
            }
            return true;
        }

        #endregion

        #region Lifecycle

        public void OnAdd()
        {
            foreach (var component in _runtimeComponents)
                component.OnAdd();
        }

        public void OnRemove()
        {
            foreach (var component in _runtimeComponents)
                component.OnRemove();
        }

        /// <summary>Called by trigger components to activate this ability.</summary>
        public void Trigger(RuntimeAbilityTriggerArgs triggerArgs)
        {
            if (_runtimeSelector != null && triggerArgs == null)
                StartAutoSelectionProcess();
            else
                UseAbility(triggerArgs);
        }

        protected virtual void OnAbilityComplete()
        {
            OwnerAbilityComponent?.OnAbilityComplete(this);
        }

        #endregion

        #region Accessors

        public RuntimeAbilitySelector GetSelectorComponent() => _runtimeSelector;
        public RuntimeAbilityCooldownComponent GetCooldownComponent() => _cooldownComponent;
        public List<RuntimeAbilityEffect> GetEffectComponents() => new List<RuntimeAbilityEffect>(_runtimeEffects);

        #endregion
    }
}
