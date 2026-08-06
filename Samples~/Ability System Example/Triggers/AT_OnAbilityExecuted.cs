using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem.Samples
{
    /// <summary>
    /// Sample: Trigger fires when any ability executes (event-driven).
    ///
    /// Parameters: none (listens via IAbilityEventBus).
    ///
    /// Projects should implement IAbilityEventBus on their character type and
    /// subscribe to the event bus in InitAsync.
    /// </summary>
    [RegisterAbilityTrigger("AT_OnAbilityExecuted")]
    public class AT_OnAbilityExecuted : AbilityTrigger
    {
        public override void Init(AbilityTriggerInitArgs initArgs) { }

        protected override RuntimeAbilityTrigger GetNewRuntimeAbilityComponent()
        {
            return new RAT_OnAbilityExecuted();
        }
    }

    public class RAT_OnAbilityExecuted : RuntimeAbilityTrigger
    {
        public override async UniTask InitAsync(AbilityTrigger ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
            await base.InitAsync(ownerAbilityComponent, ownerRuntimeAbility);
            // Integration point: subscribe to your event system.
            // E.g.: ownerRuntimeAbility.OwnerAbilityComponent.OwnerAbilityComponent?.AbilityActivated += OnAbilityFired;
            Debug.Log("[AbilitySystem Sample] OnAbilityExecuted trigger initialized — subscribe to your event system here.");
        }

        private void OnAbilityFired(RuntimeAbility ability)
        {
            Trigger();
        }
    }
}
