using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.ProjectileSystem
{
    /// <summary>
    /// The projectile system's static entry point (Candidate A's deep interface, alongside
    /// <see cref="ProjectileArgs"/>). Everything downstream — pool, stat resolution, tracking,
    /// movement, collision, pierce/bounce, despawn — is implementation behind this seam.
    ///
    /// Fused-project couplings from the original Element Conquest system live behind the seams
    /// declared here:
    ///   - pooling         → <see cref="IProjectilePool"/> (replaceable, defaults to <see cref="ProjectilePool.Default"/>)
    ///   - stats           → <see cref="IProjectileStatProvider"/> on the args
    ///   - damage outcome  → <see cref="IProjectileHitHandler"/> (implemented on targets)
    ///   - external events → <see cref="ProjectileEventBus"/> (tag-keyed)
    ///   - custom effects  → <see cref="ProjectileConditionalCustomEffect"/> on the prefab
    ///
    /// The framework references no concrete Health / Stat / Element / Pool type.
    /// </summary>
    public static class ProjectileSystem
    {
        /// <summary>The pool all launches go through. Swap in a project-specific pooling
        /// engine here without touching framework code.</summary>
        public static IProjectilePool Pool { get; set; } = ProjectilePool.Default;

        private static readonly HashSet<Projectile> _activeProjectiles = new();

        private static ProjectileSystemRunner _runner;

        private static ProjectileSystemRunner Runner
        {
            get
            {
                if (_runner == null)
                {
                    var go = new GameObject("[MioHelper ProjectileSystem]");
                    Object.DontDestroyOnLoad(go);
                    _runner = go.AddComponent<ProjectileSystemRunner>();
                }
                return _runner;
            }
        }

        #region Launch

        /// <summary>
        /// Launch a single projectile immediately. Convention: any field left at its default
        /// (0 / null / false) falls back to the prefab's serialized value.
        /// </summary>
        /// <returns>The spawned projectile, or null if the prefab could not be resolved.</returns>
        public static Projectile LaunchSingle(ProjectileArgs args)
        {
            if (args == null) return null;

            Projectile prefab = ResolvePrefab(args);
            if (prefab == null) return null;

            Vector3 position = ResolveLaunchPosition(args);
            Vector3 direction = ResolveDirection(args);
            Transform parent = ResolveParent(args);

            Projectile instance = Pool.Spawn(prefab, position, Quaternion.identity, parent);
            if (instance == null) return null;

            instance.Initialize(args);

            if (args.AttachToOwner && args.Owner != null)
                instance.transform.SetParent(args.Owner.transform, worldPositionStays: true);

            // Optional consumer seams, applied once per launch in a fixed order.
            args.StatProvider?.Apply(args, instance);
            args.OnBeforeLaunch?.Invoke(instance);
            ApplyProjectileCustomEffects(instance, args.Target);

            instance.SetDirection(direction);
            if (args.Target != null) instance.SetTrackTarget(args.Target);

            RegisterActiveProjectile(instance);
            instance.Launch();
            return instance;
        }

        /// <summary>
        /// Launch one or many projectiles. Honours ProjectileCount (concurrent spread fan),
        /// BurstCount + TimeBetweenShots (sequential volleys) and Delay. For a single immediate
        /// shot this is equivalent to <see cref="LaunchSingle"/> and returns the instance;
        /// otherwise the launch runs as a coroutine and null is returned.
        /// </summary>
        public static Projectile Launch(ProjectileArgs args)
        {
            if (args == null) return null;

            bool multi = args.ProjectileCount > 1 || args.BurstCount > 1 || args.Delay > 0f;
            if (!multi)
                return LaunchSingle(args);

            Runner.StartCoroutine(LaunchCoroutine(args));
            return null;
        }

        public static Projectile LaunchWithDelay(ProjectileArgs args, float delay)
        {
            args.Delay = delay;
            return Launch(args);
        }

        /// <summary>Convenience: spawn + launch a prefab at a world point (SpawnAtTarget mode).</summary>
        public static Projectile SpawnAtPoint(Projectile projectilePrefab, Vector3 position, GameObject owner = null)
        {
            var args = new ProjectileArgs
            {
                ProjectilePrefab = projectilePrefab,
                Owner = owner,
                LaunchPosition = position,
            };
            return LaunchSingle(args);
        }

        /// <summary>Instantiate a particle/effect at a world point and auto-destroy it.</summary>
        public static void SpawnParticle(GameObject prefab, Vector3 position, float destroyDelay)
        {
            if (prefab == null) return;
            GameObject go = Object.Instantiate(prefab, position, Quaternion.identity);
            if (destroyDelay > 0f)
                Object.Destroy(go, destroyDelay);
        }

        private static IEnumerator LaunchCoroutine(ProjectileArgs args)
        {
            if (args.Delay > 0f)
                yield return new WaitForSeconds(args.Delay);

            // Resolve the base direction once so spread has a real vector to rotate around.
            ProjectileArgs baseArgs = args.Clone();
            if (baseArgs.Direction.sqrMagnitude <= 0.0001f)
                baseArgs.Direction = ResolveDirection(baseArgs);

            for (int burst = 0; burst < baseArgs.BurstCount; burst++)
            {
                for (int i = 0; i < baseArgs.ProjectileCount; i++)
                {
                    ProjectileArgs shot = baseArgs.Clone();
                    ApplySpread(shot, i, baseArgs.ProjectileCount);
                    LaunchSingle(shot);
                }

                if (burst < baseArgs.BurstCount - 1)
                    yield return new WaitForSeconds(baseArgs.TimeBetweenShots);
            }
        }

        private static void ApplySpread(ProjectileArgs shot, int index, int total)
        {
            if (shot.Spread <= 0f) return;

            float angle;
            if (shot.RandomSpread)
            {
                angle = Random.Range(-shot.Spread / 2f, shot.Spread / 2f);
            }
            else if (total > 1)
            {
                angle = Mathf.Lerp(-shot.Spread / 2f, shot.Spread / 2f, index / (float)(total - 1));
            }
            else
            {
                angle = 0f;
            }

            if (Mathf.Abs(angle) > 0.0001f)
                shot.Direction = Quaternion.Euler(0f, 0f, angle) * shot.Direction;
        }

        #endregion

        #region Resolution

        private static Projectile ResolvePrefab(ProjectileArgs args)
        {
            if (args.ProjectilePrefab != null) return args.ProjectilePrefab;
            if (string.IsNullOrEmpty(args.ProjectilePrefabPath)) return null;

            GameObject go = Resources.Load<GameObject>(args.ProjectilePrefabPath);
            if (go == null)
            {
                Debug.LogWarning($"[ProjectileSystem] Could not load projectile prefab from Resources: '{args.ProjectilePrefabPath}'");
                return null;
            }

            Projectile projectile = go.GetComponent<Projectile>();
            if (projectile == null)
                Debug.LogWarning($"[ProjectileSystem] Resources prefab '{args.ProjectilePrefabPath}' has no Projectile component.");
            return projectile;
        }

        /// <summary>
        /// Resolve the launch direction: explicit Direction wins; otherwise aim owner→target
        /// when a target exists; otherwise owner's +X; otherwise a hardcoded default.
        /// </summary>
        public static Vector3 ResolveDirection(ProjectileArgs args)
        {
            if (args.Direction.sqrMagnitude > 0.0001f)
                return args.Direction.normalized;

            if (args.Target != null)
            {
                Vector3 from = args.LaunchPosition.sqrMagnitude > 0.0001f
                    ? args.LaunchPosition
                    : (args.Owner != null ? args.Owner.transform.position : Vector3.zero);
                Vector3 toTarget = args.Target.position - from;
                if (toTarget.sqrMagnitude > 0.0001f)
                    return toTarget.normalized;
            }

            if (args.Owner != null)
                return args.Owner.transform.right;

            return Vector3.right;
        }

        private static Vector3 ResolveLaunchPosition(ProjectileArgs args)
        {
            if (args.LaunchPosition.sqrMagnitude > 0.0001f)
                return args.LaunchPosition;

            if (args.SpawnAtTarget && args.Target != null)
                return args.Target.position;

            if (args.Owner != null)
                return args.Owner.transform.position;

            return Vector3.zero;
        }

        private static Transform ResolveParent(ProjectileArgs args)
        {
            if (args.Parent != null) return args.Parent;
            if (args.AttachToOwner && args.Owner != null) return args.Owner.transform;
            return null;
        }

        #endregion

        #region Custom Effects

        /// <summary>
        /// Invoke every <see cref="ProjectileConditionalCustomEffect"/> on the projectile prefab
        /// once per launch. Exposed so custom launchers can re-run it; <see cref="LaunchSingle"/>
        /// calls it automatically.
        /// </summary>
        public static void ApplyProjectileCustomEffects(Projectile projectile, Transform target)
        {
            if (projectile == null) return;

            var effects = projectile.GetComponents<ProjectileConditionalCustomEffect>();
            foreach (var effect in effects)
                effect.Apply(projectile, target);
        }

        #endregion

        #region Active Projectile Registry

        public static void RegisterActiveProjectile(Projectile projectile)
        {
            if (projectile != null) _activeProjectiles.Add(projectile);
        }

        public static void UnregisterActiveProjectile(Projectile projectile)
        {
            if (projectile != null) _activeProjectiles.Remove(projectile);
        }

        public static int GetActiveProjectileCount() => _activeProjectiles.Count;

        /// <summary>Destroy every active projectile owned by the given GameObject (e.g. owner death).</summary>
        public static void RemoveProjectilesOfOwner(GameObject owner)
        {
            if (owner == null) return;

            foreach (var projectile in new List<Projectile>(_activeProjectiles))
                if (projectile != null && projectile.Owner == owner)
                    projectile.Destroy();
        }

        /// <summary>Destroy every active projectile and clear the registry.</summary>
        public static void ClearAllProjectiles()
        {
            foreach (var projectile in new List<Projectile>(_activeProjectiles))
                if (projectile != null) projectile.Destroy();
            _activeProjectiles.Clear();
        }

        #endregion
    }
}
