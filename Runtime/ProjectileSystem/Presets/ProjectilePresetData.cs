using UnityEngine;

namespace MioHelper.ProjectileSystem
{
    /// <summary>Where the projectile appears on launch.</summary>
    public enum ProjectileSpawnMode
    {
        /// <summary>At the owner (launcher).</summary>
        ShootFromOwner,

        /// <summary>At the target, regardless of owner position.</summary>
        SpawnAtTarget,
    }

    /// <summary>
    /// Design-time projectile configuration (Candidate D). A data asset that gathers every
    /// knob the projectile system exposes behind one id, so a game's ability/attack layer
    /// can launch "fireball_01" without knowing the prefab or its fields. Zero settings mean
    /// "use the prefab's serialized default" — the same convention as <see cref="ProjectileArgs"/>.
    ///
    /// The framework ships zero concrete assets; consumers author these. Adding a new projectile
    /// never requires editing framework code.
    /// </summary>
    [CreateAssetMenu(menuName = "MioHelper/Projectile System/Projectile Preset")]
    public class ProjectilePresetData : ScriptableObject
    {
        [Header("Identity")]
        public string Id;

        [Header("Prefab")]
        public Projectile ProjectilePrefab;

        /// <summary>Fallback when <see cref="ProjectilePrefab"/> is null: path relative to a Resources folder.</summary>
        public string ProjectilePrefabPath;

        public ProjectileSpawnMode SpawnMode;

        [Header("Damage")]
        [Tooltip("Multiplier on the launcher's attack power: damage = AttackPower * DamagePercentage.")]
        public float DamagePercentage = 1f;

        [Header("Volley")]
        [Tooltip("Projectiles per burst (spread fan).")]
        public int ProjectileCount = 1;

        [Tooltip("Number of sequential bursts.")]
        public int BurstCount = 1;

        public float TimeBetweenShots = 0.1f;

        [Tooltip("Total fan angle in degrees; 0 = straight line.")]
        public float Spread = 30f;

        [Tooltip("When true, each shot's angle is random within the spread instead of evenly distributed.")]
        public bool RandomSpread;

        public bool CalculateDirectionOnFire;

        [Tooltip("When true, the projectile is re-parented under the owner at launch.")]
        public bool AttachToOwner;

        [Header("Behaviour Overrides")]
        [Tooltip("0 = use the prefab's serialized value.")]
        public float Damage;

        public float Size;
        public int Pierce;
        public int Bounce;
        public float LifeTime;
        public float ChanceToHit;
        public float KnockbackStrength;

        [Header("Pooling")]
        [Tooltip("Warm this many pooled instances at ApplyPreloads (usually scene load).")]
        public int PreloadAmount;
    }
}
