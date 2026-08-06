using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem.Samples
{
    /// <summary>
    /// Sample: Teleport the owner to the aim position or first target position.
    ///
    /// Parameters: [0]=TeleportMode (0=AimPosition, 1=FirstTarget)
    /// </summary>
    [RegisterAbilityEffect("AE_Teleport")]
    public class AE_Teleport : AbilityEffect
    {
        public enum TeleportMode { AimPosition, FirstTarget }
        public TeleportMode Mode = TeleportMode.AimPosition;

        protected override RuntimeAbilityEffect GetNewRuntimeAbilityComponent()
        {
            return new RAE_Teleport();
        }

        public override void Init(AbilityEffectInitArgs initArgs)
        {
            if (initArgs.Datas == null || initArgs.Datas.Length == 0) return;
            if (int.TryParse(initArgs.Datas[0], out var mode))
                Mode = (TeleportMode)mode;
        }
    }

    public class RAE_Teleport : RuntimeAbilityEffect
    {
        private AE_Teleport _config;

        public override async UniTask InitAsync(AbilityEffect ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
            await base.InitAsync(ownerAbilityComponent, ownerRuntimeAbility);
            _config = ownerAbilityComponent as AE_Teleport;
        }

        protected override void Execute(RuntimeAbilityEffectArgs abilityArgs)
        {
            Vector3 destination;

            if (_config.Mode == AE_Teleport.TeleportMode.FirstTarget
                && abilityArgs.Targets != null && abilityArgs.Targets.Count > 0)
            {
                destination = abilityArgs.Targets[0].Transform.position;
            }
            else
            {
                destination = abilityArgs.AimPosition;
            }

            if (OwnerGameObject != null)
                OwnerGameObject.transform.position = destination;
        }

        public override bool IsExecutionFinished() => true;
    }
}
