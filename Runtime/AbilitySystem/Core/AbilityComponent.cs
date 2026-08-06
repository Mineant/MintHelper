using System;
using System.Threading.Tasks;
using MioHelper;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Base class for all design-time ability components (effects, triggers, conditions, selectors).
    /// Each component has a unique UID for identity and an optional string ID for categorization.
    /// Pairs with a <see cref="RuntimeAbilityComponent"/> subclass for runtime behavior.
    /// </summary>
    [Serializable]
    public abstract class AbilityComponent
    {
        /// <summary>
        /// Globally unique ID for this component instance. Generated automatically.
        /// No two AbilityComponents share the same UID.
        /// </summary>
        public string UID = Guid.NewGuid().ToString();

        /// <summary>
        /// Optional string identifier. Components of the same type typically share the same ID
        /// (e.g. "AE_ShootProjectile"), set by their [RegisterAbilityEffect] attribute.
        /// </summary>
        public string ID;

        /// <summary>Human-readable name for designer previews.</summary>
        public string Name;

        /// <summary>Description shown in designer tooling.</summary>
        [UnityEngine.TextArea]
        public string Description;

        /// <summary>Create the runtime counterpart of this component.</summary>
        public abstract RuntimeAbilityComponent CreateBaseRuntimeAbilityComponent(RuntimeAbility runtimeAbility);
    }

    /// <summary>
    /// Typed generic base for ability components. Provides a strongly-typed factory method
    /// that creates the correct <typeparamref name="TRuntimeAbilityComponent"/>.
    /// </summary>
    public abstract class AbilityComponent<TRuntimeAbilityComponent> : AbilityComponent
        where TRuntimeAbilityComponent : RuntimeAbilityComponent, new()
    {
        public override sealed RuntimeAbilityComponent CreateBaseRuntimeAbilityComponent(RuntimeAbility runtimeAbility)
        {
            return CreateRuntimeAbilityComponent(runtimeAbility);
        }

        public virtual TRuntimeAbilityComponent CreateRuntimeAbilityComponent(RuntimeAbility runtimeAbility)
        {
            TRuntimeAbilityComponent component = GetNewRuntimeAbilityComponent();
            _ = component.InitAsync(this, runtimeAbility);
            return component;
        }

        public virtual async UniTask<TRuntimeAbilityComponent> CreateRuntimeAbilityComponentAsync(RuntimeAbility runtimeAbility)
        {
            TRuntimeAbilityComponent component = GetNewRuntimeAbilityComponent();
            await component.InitAsync(this, runtimeAbility);
            return component;
        }

        /// <summary>Factory method — subclasses return a new instance of the matching runtime type.</summary>
        protected abstract TRuntimeAbilityComponent GetNewRuntimeAbilityComponent();
    }
}
