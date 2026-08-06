using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem.Samples
{
    /// <summary>
    /// Sample: Apply a buff to the owner and/or targets.
    ///
    /// Parameters: [0]=Chance (0-1, default 1)
    /// SelfBuffs: buff IDs applied to self on activate.
    /// TargetBuffs: buff IDs applied to each target on hit.
    ///
    /// Copy and customize for your project's buff system.
    /// </summary>
    [RegisterAbilityEffect("AE_ApplyBuff")]
    public class AE_ApplyBuff : AbilityEffect
    {
        public float Chance = 1f;
        public int[] SelfBuffIds;
        public int[] TargetBuffIds;

        protected override RuntimeAbilityEffect GetNewRuntimeAbilityComponent()
        {
            return new RAE_ApplyBuff();
        }

        public override void Init(AbilityEffectInitArgs initArgs)
        {
            if (initArgs.Datas == null || initArgs.Datas.Length == 0) return;
            var chanceStr = initArgs.Datas[0];
            Chance = string.IsNullOrEmpty(chanceStr) ? 1f
                : Mathf.Clamp01(float.TryParse(chanceStr, out var c) ? c : 1f);
        }
    }

    /// <summary>
    /// Runtime for AE_ApplyBuff. Override Execute() to integrate with your buff system.
    /// </summary>
    public class RAE_ApplyBuff : RuntimeAbilityEffect
    {
        private float _chance;
        private int[] _selfBuffIds;
        private int[] _targetBuffIds;

        public override async UniTask InitAsync(AbilityEffect ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
            await base.InitAsync(ownerAbilityComponent, ownerRuntimeAbility);

            var config = ownerAbilityComponent as AE_ApplyBuff;
            _chance = config?.Chance ?? 1f;
            _selfBuffIds = config?.SelfBuffIds;
            _targetBuffIds = config?.TargetBuffIds;
        }

        protected override void Execute(RuntimeAbilityEffectArgs abilityArgs)
        {
            if (_chance < 1f && Random.value > _chance) return;

            string ownerId = Owner?.ID ?? OwnerGameObject?.name ?? "unknown";

            // Self buffs
            if (_selfBuffIds != null)
            {
                foreach (var buffId in _selfBuffIds)
                {
                    // Integration point: call your buff system.
                    // YourBuffManager.ApplyBuff(buffId, OwnerGameObject, ownerId);
                    Debug.Log($"[AbilitySystem Sample] ApplyBuff: self buff {buffId}");
                }
            }

            // Target buffs
            if (_targetBuffIds != null && abilityArgs.Targets != null)
            {
                foreach (var target in abilityArgs.Targets)
                {
                    foreach (var buffId in _targetBuffIds)
                    {
                        // YourBuffManager.ApplyBuff(buffId, target.Transform.gameObject, ownerId);
                        Debug.Log($"[AbilitySystem Sample] ApplyBuff: target buff {buffId} on {target.Transform.name}");
                    }
                }
            }
        }

        // Always consider buff application instant
        public override bool IsExecutionFinished() => true;
    }
}
