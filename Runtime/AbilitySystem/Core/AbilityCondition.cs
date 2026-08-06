using System;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Base class for all ability condition configs. Conditions define WHETHER an
    /// ability should execute when triggered.
    ///
    /// Projects subclass this to create concrete conditions (e.g. Chance, HealthThreshold).
    /// </summary>
    [Serializable]
    public abstract class AbilityCondition : AbilityComponent<RuntimeAbilityCondition>
    {
        /// <summary>Initialize this condition from data source parameters.</summary>
        public abstract void Init(AbilityConditionInitArgs initArgs);
    }

    /// <summary>
    /// Initialization arguments passed to <see cref="AbilityCondition.Init"/>.
    /// </summary>
    public class AbilityConditionInitArgs
    {
        public string[] Parameters;

        public AbilityConditionInitArgs(string[] parameters)
        {
            Parameters = parameters;
        }
    }
}
