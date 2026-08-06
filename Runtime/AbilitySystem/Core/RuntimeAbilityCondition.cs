using System;
using UnityEngine;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Runtime behavior for an ability condition. Conditions gate whether an ability
    /// actually executes when triggered. Returns true if the condition passes.
    /// </summary>
    [Serializable]
    public class RuntimeAbilityCondition : RuntimeAbilityComponent
    {
        /// <summary>Check whether this condition is satisfied. Return true to allow execution.</summary>
        public virtual bool CheckCondition(RuntimeAbilityTriggerArgs abilityArgs)
        {
            return true;
        }
    }
}
