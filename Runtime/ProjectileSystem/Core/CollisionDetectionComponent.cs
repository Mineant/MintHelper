using System;
using UnityEngine;

namespace MioHelper.ProjectileSystem
{
    /// <summary>
    /// Abstract seam decoupling the projectile from Unity's trigger callbacks, so the
    /// collision source (collider triggers, cones, manual feeds, tests) is swappable.
    /// Attach a subclass to the projectile prefab; the projectile subscribes to the
    /// raised events and runs its collision logic from there.
    /// </summary>
    public abstract class CollisionDetectionComponent : MonoBehaviour
    {
        public event Action<Collider2D> OnTriggerEnter2D;
        public event Action<Collider2D> OnTriggerStay2D;
        public event Action<Collider2D> OnTriggerExit2D;

        protected void RaiseEnter(Collider2D other) => OnTriggerEnter2D?.Invoke(other);
        protected void RaiseStay(Collider2D other) => OnTriggerStay2D?.Invoke(other);
        protected void RaiseExit(Collider2D other) => OnTriggerExit2D?.Invoke(other);
    }
}
