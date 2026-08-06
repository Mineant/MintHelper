using System;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Base class for all ability effect configs. Effects define WHAT an ability does.
    /// Each effect has an optional delay (before execution starts) and duration (how long it lasts).
    ///
    /// Projects subclass this to create concrete effects (e.g. ShootProjectile, ApplyBuff).
    /// </summary>
    [Serializable]
    public abstract class AbilityEffect : AbilityComponent<RuntimeAbilityEffect>
    {
        /// <summary>Delay in seconds before the effect executes (unscaled by attack speed).</summary>
        public float Delay;

        /// <summary>Duration of the effect in seconds. 0 means instant.</summary>
        public float Duration;

        /// <summary>Initializes this effect from data. Subclasses parse their specific parameters here.</summary>
        public abstract void Init(AbilityEffectInitArgs initArgs);

        /// <summary>Initializes common variables shared across all effects (VFX paths, buff arrays).</summary>
        public virtual void InitCommonVariables(AbilityEffectInitArgs initArgs)
        {
            Delay = initArgs.StartTime;
        }

        /// <summary>
        /// Override to create child effects that are composed within this effect.
        /// These are initialized and added to the parent Ability's effect list automatically.
        /// </summary>
        public virtual List<AbilityEffect> CreateChildEffectClasses(ref Action<List<AbilityEffect>> onInitFinish)
        {
            return null;
        }
    }

    /// <summary>
    /// Initialization arguments passed to <see cref="AbilityEffect.Init"/>.
    /// Typically populated from CSV/JSON data.
    /// </summary>
    public class AbilityEffectInitArgs
    {
        /// <summary>Start time (delay) for this effect.</summary>
        public float StartTime;

        /// <summary>String parameters parsed from the data source. Index 0 = param1, etc.</summary>
        public string[] Datas;

        public AbilityEffectInitArgs()
        {
            StartTime = 0f;
            Datas = null;
        }

        public AbilityEffectInitArgs(float startTime, string[] datas)
        {
            StartTime = startTime;
            Datas = datas;
        }

        public AbilityEffectInitArgs Clone()
        {
            return new AbilityEffectInitArgs(StartTime, Datas != null ? (string[])Datas.Clone() : null);
        }
    }
}
