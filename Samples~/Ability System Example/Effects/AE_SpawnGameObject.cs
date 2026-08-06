using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MioHelper.AbilitySystem.Samples
{
    /// <summary>
    /// Sample: Spawn a game object (VFX, pickup, summon, etc.).
    ///
    /// Parameters: [0]=PrefabPath, [1]=SpawnAtPosition (0=Owner, 1=AimPosition, 2=FirstTarget)
    /// </summary>
    [RegisterAbilityEffect("AE_SpawnGameObject")]
    public class AE_SpawnGameObject : AbilityEffect
    {
        public string PrefabPath;
        public enum SpawnLocation { Owner, AimPosition, FirstTarget }
        public SpawnLocation Location = SpawnLocation.Owner;
        public float DestroyAfterSeconds = -1f;

        protected override RuntimeAbilityEffect GetNewRuntimeAbilityComponent()
        {
            return new RAE_SpawnGameObject();
        }

        public override void Init(AbilityEffectInitArgs initArgs)
        {
        }
    }

    public class RAE_SpawnGameObject : RuntimeAbilityEffect
    {
        public override async UniTask InitAsync(AbilityEffect ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
            await base.InitAsync(ownerAbilityComponent, ownerRuntimeAbility);
        }

        protected override void Execute(RuntimeAbilityEffectArgs abilityArgs)
        {
            // Integration point: instantiate from addressables or Resources.
            // var prefab = await Addressables.LoadAssetAsync<GameObject>(_config.PrefabPath);
            // Instantiate(prefab, position, rotation);
            Debug.Log("[AbilitySystem Sample] SpawnGameObject — integrate your spawn system here.");
        }

        public override bool IsExecutionFinished() => true;
    }
}
