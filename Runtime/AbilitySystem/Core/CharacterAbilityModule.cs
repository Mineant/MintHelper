using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Optional event bus interface. Projects implement this to receive ability lifecycle events.
    /// If the owner's GameObject has a component implementing this, RuntimeAbility calls it.
    /// </summary>
    public interface IAbilityEventBus
    {
        void OnAbilityActivated(CharacterAbilityModule module, RuntimeAbility ability);
    }

    /// <summary>
    /// Optional post-processor hook. Projects implement this to inject project-specific
    /// effects or conditions during ability construction (e.g. energy costs, stat modifiers).
    /// Register via <see cref="AbilityPostProcessorRegistry"/>.
    /// </summary>
    public interface IAbilityPostProcessor
    {
        /// <summary>
        /// Called after an Ability is constructed from data. Projects can add/remove
        /// effects, triggers, or conditions here BEFORE the ability is added to a character.
        /// </summary>
        void PostProcessAbility(Ability ability);
    }

    /// <summary>
    /// Registry for ability post-processors. Projects register their processors here
    /// during initialization. Called by the Ability construction pipeline.
    /// </summary>
    public static class AbilityPostProcessorRegistry
    {
        private static List<IAbilityPostProcessor> _processors = new();

        public static void Register(IAbilityPostProcessor processor)
        {
            if (!_processors.Contains(processor))
                _processors.Add(processor);
        }

        public static void Unregister(IAbilityPostProcessor processor)
        {
            _processors.Remove(processor);
        }

        public static void Process(Ability ability)
        {
            foreach (var processor in _processors)
                processor.PostProcessAbility(ability);
        }
    }

    /// <summary>
    /// MonoBehaviour that hosts runtime abilities on a character/enemy/NPC.
    /// Manages ability slots, add/remove lifecycle, selection, and execution.
    ///
    /// Attach this to any GameObject that should have abilities. The GameObject
    /// should also have a component implementing <see cref="IAbilityOwner"/>.
    /// </summary>
    public class CharacterAbilityModule : MonoBehaviour
    {
        #region Fields

        [SerializeField] protected List<RuntimeAbility> _runtimeAbilities = new();
        [SerializeField] protected List<AbilitySlot> _abilitySlots = new();
        protected bool _isInitialized;

        public RuntimeAbility CurrentOngoingAbility { get; private set; }

        /// <summary>Fires when any ability on this character begins execution.</summary>
        public event Action<RuntimeAbility> AbilityActivated;

        /// <summary>The IAbilityOwner for this character. Resolved once from components on this GameObject.</summary>
        public IAbilityOwner Owner { get; private set; }

        #endregion

        #region Initialization & Release

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            _runtimeAbilities = new List<RuntimeAbility>();
            _abilitySlots = new List<AbilitySlot>();
            Owner = GetComponent<IAbilityOwner>();
        }

        public void Release()
        {
            if (!_isInitialized) return;

            foreach (var ability in _runtimeAbilities)
                ability?.Release();

            _runtimeAbilities.Clear();
            _abilitySlots.Clear();
            CurrentOngoingAbility = null;
            _isInitialized = false;
        }

        private void Awake()
        {
            if (!_isInitialized) Initialize();
        }

        private void OnDestroy()
        {
            Release();
        }

        private void OnDisable()
        {
            CurrentOngoingAbility?.CancelAbility();
            CurrentOngoingAbility = null;

            if (_runtimeAbilities != null)
            {
                foreach (var ability in _runtimeAbilities)
                {
                    if (ability != null && ability.IsAbilityRunning)
                        ability.CancelAbility();
                }
            }
        }

        private void Update()
        {
            if (!_isInitialized) return;

            for (int i = 0; i < _runtimeAbilities.Count; i++)
                _runtimeAbilities[i]?.OnUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!_isInitialized) return;

            for (int i = 0; i < _runtimeAbilities.Count; i++)
                _runtimeAbilities[i]?.OnFixedUpdate(Time.fixedDeltaTime);
        }

        #endregion

        #region Slot Management

        public AbilitySlot AddSlot(string slotId)
        {
            if (string.IsNullOrEmpty(slotId))
            {
                Debug.LogWarning("[AbilitySystem] Cannot add slot with null/empty ID.");
                return null;
            }

            if (_abilitySlots.Any(x => x.ID == slotId))
            {
                Debug.LogWarning($"[AbilitySystem] Slot '{slotId}' already exists.");
                return null;
            }

            var slot = new AbilitySlot(slotId, this);
            _abilitySlots.Add(slot);
            return slot;
        }

        public bool RemoveSlot(string slotId)
        {
            var slot = GetSlot(slotId);
            if (slot == null) return false;

            if (slot.RuntimeAbility != null)
                RemoveAbility(slot.RuntimeAbility);

            return _abilitySlots.Remove(slot);
        }

        public AbilitySlot GetSlot(string slotId)
        {
            var slot = _abilitySlots.FirstOrDefault(x => x.ID == slotId);
            if (slot != null && slot.RuntimeAbility != null && !_runtimeAbilities.Contains(slot.RuntimeAbility))
            {
                Debug.LogWarning($"[AbilitySystem] Stale ability reference in slot '{slotId}', clearing.");
                slot.RuntimeAbility = null;
            }
            return slot;
        }

        public async Task<bool> SetAbilityInSlot(string slotId, Ability ability, IEnumerable<string> statGroups = null)
        {
            var slot = GetSlot(slotId);
            if (slot == null)
            {
                Debug.LogWarning($"[AbilitySystem] Slot '{slotId}' not found.");
                return false;
            }

            if (slot.RuntimeAbility != null)
                RemoveAbility(slot.RuntimeAbility);

            var runtimeAbility = await AddAbilityAsync(ability, statGroups);
            if (runtimeAbility == null) return false;

            slot.RuntimeAbility = runtimeAbility;
            return true;
        }

        public bool ClearSlot(string slotId)
        {
            var slot = GetSlot(slotId);
            if (slot == null) return false;

            if (slot.RuntimeAbility != null)
            {
                RemoveAbility(slot.RuntimeAbility);
                slot.RuntimeAbility = null;
            }
            return true;
        }

        public RuntimeAbility GetAbilityFromSlot(string slotId) => GetSlot(slotId)?.RuntimeAbility;

        public bool UseAbilityInSlot(string slotId, RuntimeAbilityTriggerArgs args = null)
        {
            var slot = GetSlot(slotId);
            if (slot?.RuntimeAbility == null) return false;

            UseAbility(slot.RuntimeAbility, args);
            return true;
        }

        #endregion

        #region Ability Management

        public async UniTask<RuntimeAbility> AddAbilityAsync(Ability ability, IEnumerable<string> statGroups)
        {
            if (ability == null)
            {
                Debug.LogWarning("[AbilitySystem] Cannot add null ability.");
                return null;
            }

            if (!ability.AllowMultiple)
            {
                var existing = FindAbilities(ability.ID);
                if (existing != null && existing.Count > 0)
                {
                    Debug.LogWarning($"[AbilitySystem] Cannot add duplicate ability: {ability.ID}");
                    return null;
                }
            }

            if (!HasAbilityMetPrerequisite(ability))
            {
                Debug.LogWarning($"[AbilitySystem] Prerequisites not met for ability: {ability.ID}");
                return null;
            }

            var runtimeAbility = new RuntimeAbility();
            await runtimeAbility.InitializeAsync(this, ability);

            if (statGroups != null)
            {
                foreach (var group in statGroups)
                    runtimeAbility.AddStatGroup(group);
            }

            runtimeAbility.OnAdd();
            _runtimeAbilities.Add(runtimeAbility);
            return runtimeAbility;
        }

        public bool RemoveAbility(RuntimeAbility runtimeAbility)
        {
            if (runtimeAbility == null || !_runtimeAbilities.Contains(runtimeAbility))
            {
                Debug.LogWarning("[AbilitySystem] Cannot remove null or unregistered ability.");
                return false;
            }

            runtimeAbility.OnRemove();
            runtimeAbility.Release();
            _runtimeAbilities.Remove(runtimeAbility);
            return true;
        }

        public bool RemoveAbility(int abilityId)
        {
            var ability = FindRuntimeAbilityByOwnerAbilityId(abilityId);
            if (ability == null) return false;
            return RemoveAbility(ability);
        }

        public List<RuntimeAbility> FindAbilities(int abilityId)
        {
            return _runtimeAbilities.Where(x => x.OwnerAbility.ID == abilityId).ToList();
        }

        public RuntimeAbility FindRuntimeAbilityByOwnerAbilityId(int abilityId)
        {
            return _runtimeAbilities.FirstOrDefault(x => x.OwnerAbility.ID == abilityId);
        }

        public List<Ability> GetOwnerAbilities()
        {
            return _runtimeAbilities.Select(x => x.OwnerAbility).ToList();
        }

        public List<RuntimeAbility> GetRuntimeAbilities() => new List<RuntimeAbility>(_runtimeAbilities);

        public bool HasAbility(int abilityId)
        {
            return FindRuntimeAbilityByOwnerAbilityId(abilityId) != null;
        }

        /// <summary>
        /// Check if all prerequisite ability tags are met by currently owned abilities.
        /// Uses a cached tag set that is rebuilt on add/remove.
        /// </summary>
        public bool HasAbilityMetPrerequisite(Ability ability)
        {
            if (ability.PrerequisiteAbilityTags == null || ability.PrerequisiteAbilityTags.Length == 0)
                return true;

            RebuildCachedTags();

            foreach (string tag in ability.PrerequisiteAbilityTags)
            {
                if (!_cachedTags.Contains(tag))
                    return false;
            }
            return true;
        }

        private HashSet<string> _cachedTags = new();
        private bool _tagsDirty = true;

        private void RebuildCachedTags()
        {
            if (!_tagsDirty) return;

            _cachedTags.Clear();
            foreach (var ability in _runtimeAbilities)
            {
                if (ability?.OwnerAbility?.Tags == null) continue;
                foreach (var tag in ability.OwnerAbility.Tags)
                    _cachedTags.Add(tag);
            }
            _tagsDirty = false;
        }

        /// <summary>Invalidate the prerequisite tag cache. Call after adding/removing abilities.</summary>
        private void InvalidateTagCache() => _tagsDirty = true;

        #endregion

        #region Ability Usage

        public bool CanUseAbility(RuntimeAbility runtimeAbility)
        {
            if (runtimeAbility == null) return false;
            if (!_runtimeAbilities.Contains(runtimeAbility)) return false;
            if (CurrentOngoingAbility != null) return false;
            return true;
        }

        public void UseAbility(RuntimeAbility runtimeAbility, RuntimeAbilityTriggerArgs abilityArgs)
        {
            if (!CanUseAbility(runtimeAbility)) return;

            if (runtimeAbility.OwnerAbility.Type != AbilityType.Active)
            {
                Debug.LogWarning($"[AbilitySystem] Cannot directly use passive ability: {runtimeAbility.OwnerAbility.ID}");
                return;
            }

            CurrentOngoingAbility = runtimeAbility;
            runtimeAbility.UseAbility(abilityArgs);
        }

        public void StartAbilitySelector(RuntimeAbility runtimeAbility, Action<bool, SelectionInfo> onSelectionFinish = null)
        {
            if (!CanUseAbility(runtimeAbility)) return;

            runtimeAbility.StartSelectionProcess(onSelectionFinish ?? ((success, info) =>
            {
                if (success)
                {
                    UseAbility(runtimeAbility, new RuntimeAbilityTriggerArgs
                    {
                        Targets = info.Targets,
                        AimPosition = info.SelectionCenter,
                        AimDirection = info.SelectionDirection
                    });
                }
            }));
        }

        public void StartAutoAbilitySelector(RuntimeAbility runtimeAbility, Action<bool, SelectionInfo> onSelectionFinish = null)
        {
            if (!CanUseAbility(runtimeAbility)) return;

            runtimeAbility.StartAutoSelectionProcess(onSelectionFinish ?? ((success, info) =>
            {
                if (success)
                {
                    UseAbility(runtimeAbility, new RuntimeAbilityTriggerArgs
                    {
                        Targets = info.Targets,
                        AimPosition = info.SelectionCenter,
                        AimDirection = info.SelectionDirection
                    });
                }
            }));
        }

        public void CancelCurrentAbility()
        {
            CurrentOngoingAbility?.CancelAbility();
            CurrentOngoingAbility = null;
        }

        public void CancelAbility(RuntimeAbility runtimeAbility)
        {
            if (runtimeAbility == null || !_runtimeAbilities.Contains(runtimeAbility))
            {
                Debug.LogWarning("[AbilitySystem] Cannot cancel null or unregistered ability.");
                return;
            }
            runtimeAbility.CancelAbility();
        }

        #endregion

        #region Callbacks

        internal void NotifyAbilityActivated(RuntimeAbility ability)
        {
            AbilityActivated?.Invoke(ability);
        }

        public void OnAbilityComplete(RuntimeAbility runtimeAbility)
        {
            if (runtimeAbility == CurrentOngoingAbility)
                CurrentOngoingAbility = null;
        }

        public void OnAbilityCancel(RuntimeAbility runtimeAbility)
        {
            if (runtimeAbility == CurrentOngoingAbility)
                CurrentOngoingAbility = null;
        }

        #endregion
    }
}
