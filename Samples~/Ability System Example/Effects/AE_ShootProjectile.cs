using UnityEngine;
using Cysharp.Threading.Tasks;
using MioHelper.ProjectileSystem;

namespace MioHelper.AbilitySystem.Samples
{
    /// <summary>
    /// Sample: Shoot a projectile from a prefab path.
    ///
    /// Parameters: [0]=ProjectilePrefabPath, [1]=DamageAttackPercentage, [2]=ProjectileCount,
    /// [3]=BurstCount, [4]=Bounce, [5]=Pierce, [6]=Spread, [7]=Recoil, [8]=TimeBetweenShots,
    /// [9]=CalculateDirectionOnFire (1=yes), [10]=RandomSpread (1=yes)
    ///
    /// Copy and customize for your project's projectile system.
    /// </summary>
    [RegisterAbilityEffect("AE_ShootProjectile")]
    public class AE_ShootProjectile : AbilityEffect
    {
        public string ProjectilePrefabPath;
        public float DamageAttackPercentage = 1f;
        public float ProjectileCount = 1f;
        public float BurstCount = 1f;
        public float Bounce;
        public float Pierce;
        public float Spread = 30f;
        public float Recoil;
        public float TimeBetweenShots = 0.1f;
        public bool CalculateDirectionOnFire;
        public bool RandomSpread;

        protected override RuntimeAbilityEffect GetNewRuntimeAbilityComponent()
        {
            return new RAE_ShootProjectile();
        }

        public override void Init(AbilityEffectInitArgs initArgs)
        {
            if (initArgs.Datas == null) return;

            ProjectilePrefabPath = GetParam(initArgs, 0);
            DamageAttackPercentage = ParseFloat(GetParam(initArgs, 1), 1f);
            ProjectileCount = ParseFloat(GetParam(initArgs, 2), 1f);
            BurstCount = ParseFloat(GetParam(initArgs, 3), 1f);
            Bounce = ParseFloat(GetParam(initArgs, 4), 0f);
            Pierce = ParseFloat(GetParam(initArgs, 5), 0f);
            Spread = ParseFloat(GetParam(initArgs, 6), 30f);
            Recoil = ParseFloat(GetParam(initArgs, 7), 0f);
            TimeBetweenShots = ParseFloat(GetParam(initArgs, 8), 0.1f);
            CalculateDirectionOnFire = ParseFloat(GetParam(initArgs, 9), 0) == 1;
            RandomSpread = ParseFloat(GetParam(initArgs, 10), 0) == 1;
        }

        private static string GetParam(AbilityEffectInitArgs args, int index)
        {
            return index < args.Datas.Length ? args.Datas[index] : "";
        }

        private static float ParseFloat(string s, float defaultValue)
        {
            if (string.IsNullOrEmpty(s)) return defaultValue;
            return float.TryParse(s, out var v) ? v : defaultValue;
        }
    }

    /// <summary>
    /// Runtime for AE_ShootProjectile. Override Execute() to integrate with your projectile system.
    /// </summary>
    public class RAE_ShootProjectile : RuntimeAbilityEffect
    {
        protected AE_ShootProjectile _config;

        public override async UniTask InitAsync(AbilityEffect ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
            await base.InitAsync(ownerAbilityComponent, ownerRuntimeAbility);
            _config = ownerAbilityComponent as AE_ShootProjectile;
        }

        protected override void Execute(RuntimeAbilityEffectArgs abilityArgs)
        {
            if (_config == null) return;

            GameObject ownerGO = OwnerGameObject != null ? OwnerGameObject : Owner?.GameObject;
            if (ownerGO == null)
            {
                Debug.LogWarning("[AbilitySystem Sample] AE_ShootProjectile has no owner GameObject.");
                return;
            }

            var args = new ProjectileArgs
            {
                ProjectilePrefabPath = _config.ProjectilePrefabPath,
                Owner = ownerGO,
                ProjectileCount = Mathf.Max(1, Mathf.RoundToInt(_config.ProjectileCount)),
                BurstCount = Mathf.Max(1, Mathf.RoundToInt(_config.BurstCount)),
                TimeBetweenShots = _config.TimeBetweenShots,
                Spread = _config.Spread,
                RandomSpread = _config.RandomSpread,
                Pierce = Mathf.RoundToInt(_config.Pierce),
                BounceCount = Mathf.RoundToInt(_config.Bounce),
            };

            // Damage = attack power × percentage, resolved through the IProjectileAttackSource seam.
            if (_config.DamageAttackPercentage > 0f
                && ownerGO.TryGetComponent(out IProjectileAttackSource attackSource))
            {
                args.Damage = attackSource.AttackPower * _config.DamageAttackPercentage;
            }

            // Aim: an explicit aim direction wins; otherwise track the first selected target.
            if (abilityArgs != null)
            {
                if (HasFiniteDirection(abilityArgs.AimDirection))
                {
                    args.Direction = abilityArgs.AimDirection;
                }
                else if (abilityArgs.Targets != null && abilityArgs.Targets.Count > 0)
                {
                    var target = abilityArgs.Targets[0];
                    if (target != null && target.Transform != null)
                        args.Target = target.Transform;
                }
            }

            ProjectileSystem.Launch(args);
        }

        private static bool HasFiniteDirection(Vector3 direction)
        {
            return float.IsFinite(direction.x) && float.IsFinite(direction.y);
        }
    }
}
