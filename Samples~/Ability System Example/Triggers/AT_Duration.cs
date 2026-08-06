using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem.Samples
{
    /// <summary>
    /// Sample: Trigger fires on a repeating timer.
    ///
    /// Parameters: [0]=IntervalSeconds
    /// </summary>
    [RegisterAbilityTrigger("AT_Duration")]
    public class AT_Duration : AbilityTrigger
    {
        public float IntervalSeconds = 1f;

        public override void Init(AbilityTriggerInitArgs initArgs)
        {
            if (initArgs.Parameters != null && initArgs.Parameters.Length > 0)
                float.TryParse(initArgs.Parameters[0], out IntervalSeconds);
        }

        protected override RuntimeAbilityTrigger GetNewRuntimeAbilityComponent()
        {
            return new RAT_Duration();
        }
    }

    /// <summary>
    /// Runtime for AT_Duration. Fires the owning ability every IntervalSeconds.
    /// A real implementation would use Update or a coroutine to track elapsed time.
    /// </summary>
    public class RAT_Duration : RuntimeAbilityTrigger
    {
        private float _interval;
        private float _elapsed;

        public override async UniTask InitAsync(AbilityTrigger ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
            await base.InitAsync(ownerAbilityComponent, ownerRuntimeAbility);
            _interval = (ownerAbilityComponent as AT_Duration)?.IntervalSeconds ?? 1f;
        }

        public override void OnUpdate(float deltaTime)
        {
            _elapsed += deltaTime;
            if (_elapsed >= _interval)
            {
                _elapsed = 0f;
                Trigger();
            }
        }
    }
}
