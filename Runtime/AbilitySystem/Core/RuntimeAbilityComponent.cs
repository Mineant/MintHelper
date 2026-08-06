using System;
using System.Threading.Tasks;
using UnityEngine;
using MioHelper;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Base class for all runtime ability components. Created from a design-time
    /// <see cref="AbilityComponent"/> when an ability is added to a character.
    ///
    /// Lifecycle: InitAsync → OnFinishAllRuntimeAbilityComponentsInit → OnAdd → Update loop → OnRemove → Release
    /// </summary>
    [Serializable]
    public abstract class RuntimeAbilityComponent
    {
        /// <summary>Globally unique ID for this runtime component instance.</summary>
        public string ID = Guid.NewGuid().ToString();

        /// <summary>The design-time component that created this runtime instance.</summary>
        public AbilityComponent BaseOwnerAbilityComponent { get; private set; }

        /// <summary>The RuntimeAbility this component belongs to.</summary>
        public RuntimeAbility OwnerRuntimeAbility { get; private set; }

        /// <summary>Convenience accessor for the character ability module.</summary>
        public CharacterAbilityModule OwnerCharacterAbilityModule => OwnerRuntimeAbility?.OwnerAbilityComponent;

        /// <summary>The entity that owns this ability (via IAbilityOwner interface).</summary>
        public IAbilityOwner Owner { get; private set; }

        /// <summary>The GameObject this component operates on.</summary>
        public GameObject OwnerGameObject => OwnerCharacterAbilityModule?.gameObject;

        /// <summary>Whether this component has been initialized.</summary>
        public bool Initialized { get; protected set; }

        public RuntimeAbilityComponent()
        {
            Initialized = false;
        }

        /// <summary>
        /// Initialize this runtime component. Sets up references to the design-time component
        /// and the owning runtime ability.
        /// </summary>
        public virtual async UniTask InitAsync(AbilityComponent ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
            if (Initialized)
            {
                Debug.LogWarning($"[AbilitySystem] RuntimeAbilityComponent already initialized. ID: {ID}");
            }

            BaseOwnerAbilityComponent = ownerAbilityComponent;
            OwnerRuntimeAbility = ownerRuntimeAbility;
            Owner = ownerRuntimeAbility?.OwnerAbilityComponent?.GetComponent<IAbilityOwner>();
            Initialized = true;
        }

        /// <summary>Release resources and clean up.</summary>
        public virtual void Release()
        {
            Initialized = false;
        }

        /// <summary>Called when the ability is added to a character.</summary>
        public virtual void OnAdd() { }

        /// <summary>Called when the ability is removed from a character.</summary>
        public virtual void OnRemove() { }

        /// <summary>Called after ALL runtime components in the ability have finished initializing.</summary>
        public virtual void OnFinishAllRuntimeAbilityComponentsInit() { }

        /// <summary>Per-frame update, driven by CharacterAbilityModule.</summary>
        public virtual void OnUpdate(float deltaTime) { }

        /// <summary>Per-fixed-update, driven by CharacterAbilityModule.</summary>
        public virtual void OnFixedUpdate(float fixedDeltaTime) { }
    }

    /// <summary>
    /// Typed generic base for runtime ability components. Provides strongly-typed access
    /// to the design-time <typeparamref name="TAbilityComponent"/>.
    /// </summary>
    public abstract class RuntimeAbilityComponent<TAbilityComponent> : RuntimeAbilityComponent
        where TAbilityComponent : AbilityComponent
    {
        /// <summary>Typed accessor for the design-time component.</summary>
        public TAbilityComponent OwnerAbilityComponent => (TAbilityComponent)BaseOwnerAbilityComponent;

        public override sealed async UniTask InitAsync(AbilityComponent ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
            await base.InitAsync(ownerAbilityComponent, ownerRuntimeAbility);
            await InitAsync(OwnerAbilityComponent, ownerRuntimeAbility);
        }

        /// <summary>Typed initialization. Subclasses override this instead of the untyped version.</summary>
        public virtual async UniTask InitAsync(TAbilityComponent ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
        }
    }
}
