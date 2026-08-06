using System;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.ProjectileSystem
{
    public abstract class ProjectileEventArgs
    {
        public Projectile Projectile;
    }

    public class ProjectileLaunchedEventArgs : ProjectileEventArgs { }

    public class ProjectileHitEventArgs : ProjectileEventArgs
    {
        public GameObject HitObject;
    }

    public class ProjectileKilledEventArgs : ProjectileEventArgs
    {
        public GameObject KilledObject;
    }

    public class ProjectileDestroyedEventArgs : ProjectileEventArgs { }

    /// <summary>
    /// The interface the projectile system offers to observers (Candidate E): a small,
    /// tag-keyed event bus. Projectiles carry string tags; listeners subscribe per tag so
    /// e.g. ability triggers can filter for a specific projectile. Events fire synchronously
    /// at the moment they happen (launch / hit / kill / destroy).
    ///
    /// Storage keeps the original <see cref="Delegate"/> so add/remove compare by reference.
    /// </summary>
    public static class ProjectileEventBus
    {
        // Dictionary<argsTypeName, Dictionary<tag, List<Delegate>>>
        private static readonly Dictionary<string, Dictionary<string, List<Delegate>>> _table = new();

        /// <summary>Remove all listeners and state. Call when tearing down a scene/run.</summary>
        public static void ClearAll()
        {
            _table.Clear();
        }

        public static void AddListener<TArgs>(string tag, Action<TArgs> listener) where TArgs : ProjectileEventArgs
        {
            if (string.IsNullOrEmpty(tag) || listener == null) return;

            var argsName = typeof(TArgs).Name;
            if (!_table.TryGetValue(argsName, out var byTag))
                _table[argsName] = byTag = new Dictionary<string, List<Delegate>>();

            if (!byTag.TryGetValue(tag, out var listeners))
                byTag[tag] = listeners = new List<Delegate>();

            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        public static void RemoveListener<TArgs>(string tag, Action<TArgs> listener) where TArgs : ProjectileEventArgs
        {
            if (string.IsNullOrEmpty(tag) || listener == null) return;

            var argsName = typeof(TArgs).Name;
            if (_table.TryGetValue(argsName, out var byTag) && byTag.TryGetValue(tag, out var listeners))
                listeners.Remove(listener);
        }

        public static void Trigger<TArgs>(string tag, TArgs args) where TArgs : ProjectileEventArgs
        {
            var argsName = typeof(TArgs).Name;
            if (!_table.TryGetValue(argsName, out var byTag) || !byTag.TryGetValue(tag, out var listeners)) return;

            // Iterate a copy so listeners may add/remove during dispatch.
            foreach (var listener in new List<Delegate>(listeners))
            {
                if (listener is Action<TArgs> typed)
                    typed.Invoke(args);
            }
        }

        /// <summary>Trigger for every tag the projectile carries.</summary>
        public static void Trigger<TArgs>(Projectile projectile, TArgs args) where TArgs : ProjectileEventArgs
        {
            if (projectile == null || projectile.Tags == null) return;
            foreach (var tag in projectile.Tags)
                Trigger(tag, args);
        }
    }
}
