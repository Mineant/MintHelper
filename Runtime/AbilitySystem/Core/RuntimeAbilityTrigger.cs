using System;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Runtime behavior for an ability trigger. Triggers fire the owning ability
    /// when their subscribed event occurs.
    /// </summary>
    [Serializable]
    public class RuntimeAbilityTrigger : RuntimeAbilityComponent<AbilityTrigger>
    {
        /// <summary>
        /// Fire the trigger — invokes the owning RuntimeAbility with the given arguments.
        /// </summary>
        public virtual void Trigger(RuntimeAbilityTriggerArgs triggerArgs = null)
        {
            OwnerRuntimeAbility?.Trigger(triggerArgs);
        }
    }
}
