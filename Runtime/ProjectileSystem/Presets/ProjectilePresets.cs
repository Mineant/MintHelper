using UnityEngine;

namespace MioHelper.ProjectileSystem
{
    /// <summary>
    /// Thin preset-driven launcher (Candidate D). Builds a <see cref="ProjectileArgs"/> from a
    /// <see cref="ProjectilePresetData"/> and hands it to <see cref="ProjectileSystem.Launch"/>.
    ///
    /// A game's ability/attack layer calls one of the Launch overloads; adding a new projectile
    /// is authoring a preset asset, never editing framework code. The preset database is resolved
    /// from Resources ("Projectile Preset Database") when not passed explicitly — drop the asset
    /// in a Resources folder, or pass the reference straight in.
    /// </summary>
    public static class ProjectilePresets
    {
        private const string DefaultDatabasePath = "Projectile Preset Database";

        /// <summary>
        /// Launch a preset by id.
        /// </summary>
        /// <param name="database">Preset lookup. Resolved from Resources if null.</param>
        /// <param name="id">Preset id to launch.</param>
        /// <param name="owner">Launcher GameObject (position, direction, ownership).</param>
        /// <param name="attackPower">
        /// Base attack power; damage = attackPower × DamagePercentage. When ≤ 0, resolved from
        /// the owner's <see cref="IProjectileAttackSource"/>; when neither is present, 1.0.
        /// </param>
        /// <param name="target">Optional aim target (position/direction).</param>
        /// <param name="direction">Optional explicit direction; otherwise aim at target or owner facing.</param>
        public static Projectile Launch(ProjectilePresetDatabase database, string id, GameObject owner,
            float attackPower = 0f, Transform target = null, Vector3 direction = default)
        {
            ProjectilePresetData preset = (database != null ? database : LoadDatabase()).Get(id);
            if (preset == null)
            {
                Debug.LogWarning($"[ProjectilePresets] No preset with id '{id}'.");
                return null;
            }

            return Launch(preset, owner, attackPower, target, direction);
        }

        public static Projectile Launch(ProjectilePresetData preset, GameObject owner,
            float attackPower = 0f, Transform target = null, Vector3 direction = default)
        {
            if (preset == null) return null;

            var args = new ProjectileArgs
            {
                ProjectilePrefab = preset.ProjectilePrefab,
                ProjectilePrefabPath = preset.ProjectilePrefabPath,
                Owner = owner,
                Target = target,
                Direction = direction,
                SpawnAtTarget = preset.SpawnMode == ProjectileSpawnMode.SpawnAtTarget,
                AttachToOwner = preset.AttachToOwner,
                ProjectileCount = preset.ProjectileCount,
                BurstCount = preset.BurstCount,
                TimeBetweenShots = preset.TimeBetweenShots,
                Spread = preset.Spread,
                RandomSpread = preset.RandomSpread,
                Damage = preset.Damage,
                Size = preset.Size,
                Pierce = preset.Pierce,
                BounceCount = preset.Bounce,
                LifeTime = preset.LifeTime,
                ChanceToHit = preset.ChanceToHit,
                KnockbackStrength = preset.KnockbackStrength,
                StatProvider = owner != null ? owner.GetComponent<IProjectileStatProvider>() : null,
            };

            if (preset.DamagePercentage > 0f)
            {
                float resolvedPower = attackPower;
                if (resolvedPower <= 0f && owner != null && owner.TryGetComponent(out IProjectileAttackSource source))
                    resolvedPower = source.AttackPower;
                if (resolvedPower <= 0f) resolvedPower = 1f;

                args.Damage = resolvedPower * preset.DamagePercentage;
            }

            return ProjectileSystem.Launch(args);
        }

        /// <summary>Warm the pool for every preset in the database (call at scene load).</summary>
        public static void ApplyPreloads(ProjectilePresetDatabase database)
        {
            if (database == null || database.Presets == null) return;
            foreach (var preset in database.Presets)
                ApplyPreloads(preset);
        }

        public static void ApplyPreloads(ProjectilePresetData preset)
        {
            if (preset == null || preset.PreloadAmount <= 0) return;

            Projectile prefab = preset.ProjectilePrefab;
            if (prefab == null && !string.IsNullOrEmpty(preset.ProjectilePrefabPath))
            {
                var go = Resources.Load<GameObject>(preset.ProjectilePrefabPath);
                if (go != null) prefab = go.GetComponent<Projectile>();
            }

            if (prefab == null) return;

            int target = Mathf.Max(preset.PreloadAmount, ProjectileSystem.Pool.PreloadCount(prefab));
            ProjectileSystem.Pool.SetPreloadCount(prefab, target);
        }

        private static ProjectilePresetDatabase LoadDatabase()
        {
            var database = Resources.Load<ProjectilePresetDatabase>(DefaultDatabasePath);
            if (database == null)
                Debug.LogWarning("[ProjectilePresets] No database found. Create one and drop it in a Resources folder, or pass it explicitly.");
            return database;
        }
    }
}
