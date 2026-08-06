using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem.Samples
{
    /// <summary>
    /// Sample: Chance-based condition. Passes randomly based on the configured probability.
    ///
    /// Parameters: [0]=Chance (0-1, default 1.0)
    /// </summary>
    [RegisterAbilityCondition("AC_Chance")]
    public class AC_Chance : AbilityCondition
    {
        public float Chance = 1f;

        public override void Init(AbilityConditionInitArgs initArgs)
        {
            if (initArgs.Parameters != null && initArgs.Parameters.Length > 0)
                Chance = Mathf.Clamp01(float.TryParse(initArgs.Parameters[0], out var c) ? c : 1f);
        }

        protected override RuntimeAbilityCondition GetNewRuntimeAbilityComponent()
        {
            return new RAC_Chance();
        }
    }

    public class RAC_Chance : RuntimeAbilityCondition
    {
        private float _chance;

        public override async UniTask InitAsync(AbilityComponent ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
            await base.InitAsync(ownerAbilityComponent, ownerRuntimeAbility);
            _chance = (ownerAbilityComponent as AC_Chance)?.Chance ?? 1f;
        }

        public override bool CheckCondition(RuntimeAbilityTriggerArgs abilityArgs)
        {
            if (_chance <= 0f) return false;
            if (_chance >= 1f) return true;
            return UnityEngine.Random.value <= _chance;
        }
    }
}
