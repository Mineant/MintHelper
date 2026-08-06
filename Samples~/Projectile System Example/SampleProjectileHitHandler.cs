using UnityEngine;
using MioHelper.ProjectileSystem;

namespace MioHelper.Samples.ProjectileSystem
{
    /// <summary>
    /// Sample implementation of <see cref="IProjectileHitHandler"/> — the seam where the
    /// framework hands a hit to the consumer and the consumer decides the outcome (damage,
    /// knockback, death, damage numbers — all project concerns).
    ///
    /// This is the canonical "adapter #2": EC's Health-based handler is adapter #1, this
    /// standalone MonoBehaviour is #2, which is what makes the seam real rather than hypothetical.
    /// Attach to a target GameObject that has a trigger collider.
    /// </summary>
    public class SampleProjectileHitHandler : MonoBehaviour, IProjectileHitHandler
    {
        public float MaxHealth = 100f;

        public float CurrentHealth { get; private set; }

        private void Awake()
        {
            CurrentHealth = MaxHealth;
        }

        public bool OnProjectileHit(ProjectileHitContext context)
        {
            if (CurrentHealth <= 0f) return false;

            CurrentHealth -= context.Damage;
            Debug.Log($"[SampleProjectileHitHandler] {name} took {context.Damage} damage ({CurrentHealth}/{MaxHealth} HP).");

            if (CurrentHealth <= 0f)
            {
                Debug.Log($"[SampleProjectileHitHandler] {name} was destroyed.");
                Destroy(gameObject);
                return true; // killed → the framework fires the ProjectileKilled event
            }

            return false;
        }
    }
}
