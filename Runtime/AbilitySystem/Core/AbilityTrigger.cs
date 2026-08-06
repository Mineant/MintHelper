using System;
using System.Collections.Generic;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Base class for all ability trigger configs. Triggers define WHEN an ability fires.
    /// Triggers subscribe to game events and invoke the ability when conditions are met.
    ///
    /// Projects subclass this to create concrete triggers (e.g. OnProjectileHit, OnTimer, OnBuffApplied).
    /// </summary>
    [Serializable]
    public abstract class AbilityTrigger : AbilityComponent<RuntimeAbilityTrigger>
    {
        /// <summary>Initialize this trigger from data source parameters.</summary>
        public abstract void Init(AbilityTriggerInitArgs initArgs);
    }

    /// <summary>
    /// Initialization arguments passed to <see cref="AbilityTrigger.Init"/>.
    /// </summary>
    public class AbilityTriggerInitArgs
    {
        public string[] Parameters;

        public AbilityTriggerInitArgs(string[] parameters)
        {
            Parameters = parameters;
        }
    }
}
