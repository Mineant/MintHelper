using UnityEngine;

namespace MioHelper.ProjectileSystem
{
    /// <summary>
    /// Extension seam: attach a subclass to a projectile prefab to adapt each instance at
    /// launch (VFX swap, size/damage tweaks keyed to owner stats, conditional behaviour, etc.).
    /// Invoked by <see cref="ProjectileSystem"/> once per launch, before the direction is applied.
    /// </summary>
    public abstract class ProjectileConditionalCustomEffect : MonoBehaviour
    {
        public abstract void Apply(Projectile projectile, Transform target);
    }
}
