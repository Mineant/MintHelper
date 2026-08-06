using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.ProjectileSystem
{
    /// <summary>
    /// The pool seam (Candidate C): the projectile system's single instantiation point.
    /// <see cref="ProjectileSystem"/> never calls Object.Instantiate directly — it goes
    /// through this interface so a project can plug in its own pooling engine (e.g. EC's
    /// CrystalFramework ObjectPoolsManager) without touching the framework.
    /// </summary>
    public interface IProjectilePool
    {
        Projectile Spawn(Projectile prefab, Vector3 position, Quaternion rotation, Transform parent);
        void Despawn(Projectile projectile);
        void SetPreloadCount(Projectile prefab, int count);
        int PreloadCount(Projectile prefab);
        void ClearAll();
    }

    /// <summary>
    /// Default prefab-keyed stack pool. Spawn reuses a pooled (inactive) instance or
    /// instantiates a fresh copy; Despawn deactivates and re-pools it under a hidden root.
    /// Preload warms the pool to a target count at startup.
    ///
    /// Pool safety: instances are fully re-initialized by <see cref="Projectile.Initialize"/>
    /// on every spawn, so per-shot state never leaks between uses.
    /// </summary>
    public class ProjectilePool : IProjectilePool
    {
        public static readonly ProjectilePool Default = new ProjectilePool();

        private readonly Dictionary<Projectile, Stack<Projectile>> _pools = new();
        private readonly Dictionary<Projectile, int> _preloadCounts = new();
        private Transform _root;

        public Transform Root
        {
            get
            {
                if (_root == null)
                {
                    var go = new GameObject("[MioHelper ProjectilePool]");
                    Object.DontDestroyOnLoad(go);
                    _root = go.transform;
                }
                return _root;
            }
        }

        public Projectile Spawn(Projectile prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (prefab == null)
            {
                Debug.LogError("[ProjectileSystem] ProjectilePool.Spawn: prefab is null.");
                return null;
            }

            Projectile instance = TryPop(prefab);

            if (instance == null)
            {
                var go = Object.Instantiate(prefab.gameObject, position, rotation, parent);
                instance = go.GetComponent<Projectile>();
                if (instance == null)
                {
                    Debug.LogError($"[ProjectileSystem] Prefab '{prefab.name}' has no Projectile component.");
                    Object.Destroy(go);
                    return null;
                }
            }
            else
            {
                var t = instance.transform;
                t.SetParent(parent, worldPositionStays: false);
                t.SetPositionAndRotation(position, rotation);
            }

            instance.PrefabSource = prefab;
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Despawn(Projectile projectile)
        {
            if (projectile == null || projectile.gameObject == null) return;

            Projectile prefab = projectile.PrefabSource;
            if (prefab == null)
            {
                // Not spawned through this pool; destroy outright to avoid leaks.
                Object.Destroy(projectile.gameObject);
                return;
            }

            projectile.transform.SetParent(Root, worldPositionStays: false);
            projectile.gameObject.SetActive(false);

            if (!_pools.TryGetValue(prefab, out var stack))
                _pools[prefab] = stack = new Stack<Projectile>();
            stack.Push(projectile);
        }

        public void SetPreloadCount(Projectile prefab, int count)
        {
            if (prefab == null) return;

            _preloadCounts[prefab] = count;

            if (!_pools.TryGetValue(prefab, out var stack))
                _pools[prefab] = stack = new Stack<Projectile>();

            int missing = count - stack.Count;
            for (int i = 0; i < missing; i++)
            {
                var go = Object.Instantiate(prefab.gameObject, Root);
                var p = go.GetComponent<Projectile>();
                if (p == null)
                {
                    Object.Destroy(go);
                    continue;
                }
                p.PrefabSource = prefab;
                go.SetActive(false);
                stack.Push(p);
            }
        }

        public int PreloadCount(Projectile prefab)
        {
            return prefab != null && _preloadCounts.TryGetValue(prefab, out var count) ? count : 0;
        }

        /// <summary>Destroy every pooled instance. Active (in-scene) projectiles are untouched.</summary>
        public void ClearAll()
        {
            foreach (var kv in _pools)
            {
                while (kv.Value.Count > 0)
                {
                    var p = kv.Value.Pop();
                    if (p != null && p.gameObject != null)
                        Object.Destroy(p.gameObject);
                }
            }
        }

        private Projectile TryPop(Projectile prefab)
        {
            if (!_pools.TryGetValue(prefab, out var stack)) return null;

            while (stack.Count > 0)
            {
                var instance = stack.Pop();
                if (instance != null && instance.gameObject != null)
                    return instance;
            }
            return null;
        }
    }

    /// <summary>
    /// Hidden MonoBehaviour used to run the projectile system's coroutines (bursts, delays)
    /// from its static entry points. Framework-internal — not a seam.
    /// </summary>
    internal sealed class ProjectileSystemRunner : MonoBehaviour { }
}
