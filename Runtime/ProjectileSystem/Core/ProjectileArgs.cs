using System;
using UnityEngine;

namespace MioHelper.ProjectileSystem
{
    /// <summary>
    /// The rich data-transfer object that is the projectile system's deep interface (Candidate A).
    /// Build one of these and hand it to <see cref="ProjectileSystem.Launch"/> or
    /// <see cref="ProjectileSystem.LaunchSingle"/> — everything downstream (pooling, stat
    /// resolution, tracking, movement, collision, pierce/bounce, despawn) is implementation
    /// behind that seam.
    ///
    /// Convention: a value of 0 (or null / false) means "use the prefab's serialized default";
    /// a positive value overrides it for this launch. Exceptions: ProjectileCount/BurstCount
    /// default to 1, TimeBetweenShots to 0.1.
    /// </summary>
    [Serializable]
    public class ProjectileArgs
    {
        public Projectile ProjectilePrefab;

        /// <summary>Fallback when <see cref="ProjectilePrefab"/> is null: path relative to a Resources folder.</summary>
        public string ProjectilePrefabPath;

        public GameObject Owner;
        public Vector3 Direction;
        public Vector3 LaunchPosition;
        public Transform Target;
        public Transform Parent;

        /// <summary>
        /// When true and no explicit LaunchPosition is set, spawn at the target's position
        /// instead of the owner's (the preset's SpawnMode == SpawnAtTarget).
        /// </summary>
        public bool SpawnAtTarget;

        /// <summary>When true, the projectile is re-parented under the owner at launch.</summary>
        public bool AttachToOwner;

        public int ProjectileCount = 1;
        public int BurstCount = 1;
        public float TimeBetweenShots = 0.1f;
        public float Spread;
        public bool RandomSpread;

        /// <summary>Seconds to wait before launching (burst/delay path).</summary>
        public float Delay;

        public float Damage;
        public float Size;
        public int Pierce;
        public int BounceCount;
        public float BounceDamageIncrease;
        public float LifeTime;
        public float ChanceToHit;
        public float KnockbackStrength;

        public IProjectileStatProvider StatProvider;
        public Action<Projectile> OnBeforeLaunch;

        public ProjectileArgs Clone()
        {
            return (ProjectileArgs)MemberwiseClone();
        }
    }
}
