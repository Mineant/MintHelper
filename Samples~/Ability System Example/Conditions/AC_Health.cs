using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem.Samples
{
    /// <summary>
    /// Sample: Health threshold condition. Passes when the owner's current health
    /// is above or below a specified percentage.
    ///
    /// Parameters: [0]=Threshold (0-1 percentage), [1]=AboveOrBelow (0=Above, 1=Below)
    /// </summary>
    [RegisterAbilityCondition("AC_Health")]
    public class AC_Health : AbilityCondition
    {
        public float Threshold = 0.5f;
        public bool Below = true; // true = trigger when BELOW threshold

        public override void Init(AbilityConditionInitArgs initArgs)
        {
            if (initArgs.Parameters == null) return;
            if (initArgs.Parameters.Length > 0)
                Threshold = Mathf.Clamp01(float.TryParse(initArgs.Parameters[0], out var t) ? t : 0.5f);
            if (initArgs.Parameters.Length > 1)
                Below = int.TryParse(initArgs.Parameters[1], out var b) ? b == 1 : true;
        }

        protected override RuntimeAbilityCondition GetNewRuntimeAbilityComponent()
        {
            return new RAC_Health();
        }
    }

    public class RAC_Health : RuntimeAbilityCondition
    {
        private float _threshold;
        private bool _below;

        public override async UniTask InitAsync(AbilityComponent ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
            await base.InitAsync(ownerAbilityComponent, ownerRuntimeAbility);
            var config = ownerAbilityComponent as AC_Health;
            _threshold = config?.Threshold ?? 0.5f;
            _below = config?.Below ?? true;
        }

        public override bool CheckCondition(RuntimeAbilityTriggerArgs abilityArgs)
        {
            // Integration point: read health from your entity's health component via IAbilityOwner.GetBehaviour<T>()
            // Example:
            //   var health = Owner?.GetBehaviour<IHealth>();
            //   if (health == null) return true; // no health component = always pass
            //   return _below ? health.CurrentRatio <= _threshold : health.CurrentRatio >= _threshold;
            Debug.Log("[AbilitySystem Sample] AC_Health — integrate your health system via IAbilityOwner.GetBehaviour<T>().");
            return true;
        }
    }
}
