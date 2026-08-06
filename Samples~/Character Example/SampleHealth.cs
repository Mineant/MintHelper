using System;
using UnityEngine;
using MioHelper.ProjectileSystem;

namespace MioHelper.Samples.CharacterSystem
{
    /// <summary>
    /// Sample health/damageable. Implements <see cref="IProjectileHitHandler"/> so a projectile hits
    /// it turnkey — the "adapter #3" for the projectile hit seam (EC's Health is #1,
    /// SampleProjectileHitHandler is #2). Attach to a SampleCharacter (or any entity) with a collider.
    ///
    /// Keeps the same shape as the package's seam contract: the handler owns damage/death/knockback;
    /// the framework owns nothing about health.
    /// </summary>
    public class SampleHealth : MonoBehaviour, IProjectileHitHandler
    {
        [Tooltip("Starting / max health.")]
        public float MaxHealth = 100f;

        [Tooltip("If true, the GameObject is destroyed after dying.")]
        public bool DespawnOnDeath = true;

        [Tooltip("Seconds between death and despawn (if DespawnOnDeath).")]
        public float DeathDuration = 1f;

        public float CurrentHealth { get; protected set; }
        public bool IsAlive { get; protected set; } = true;
        public bool IsInvincible { get; protected set; }

        /// <summary>Fires with (current, max) whenever health changes.</summary>
        public event Action<float, float> OnHealthChanged;
        public event Action<DamageArgs> OnDamaged;
        public event Action OnDeath;
        public event Action OnRevive;

        protected virtual void Awake()
        {
            SetHealth(MaxHealth);
        }

        public virtual void Damage(DamageArgs args)
        {
            if (!IsAlive || IsInvincible || args == null) return;

            // Chance-to-hit miss roll.
            if (args.ChanceToHit < 1f && UnityEngine.Random.value > args.ChanceToHit) return;

            float damage = args.Damage;
            if (args.CritChance > 0f && UnityEngine.Random.value <= args.CritChance)
                damage *= args.CritDamageMultiplier;

            ChangeHealth(-damage);
            OnDamaged?.Invoke(args);

            if (CurrentHealth <= 0f)
                Kill();
        }

        public virtual void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            ChangeHealth(amount);
        }

        public virtual void SetHealth(float value)
        {
            CurrentHealth = Mathf.Clamp(value, 0f, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public virtual void ChangeHealth(float delta) => SetHealth(CurrentHealth + delta);

        public virtual void Kill()
        {
            if (!IsAlive) return;
            IsAlive = false;
            OnDeath?.Invoke();

            if (DespawnOnDeath)
                Destroy(gameObject, DeathDuration);
        }

        public virtual void Revive()
        {
            if (IsAlive) return;
            IsAlive = true;
            SetHealth(MaxHealth);
            OnRevive?.Invoke();
        }

        public virtual void BecomeInvincible(float duration)
        {
            IsInvincible = true;
            StopAllCoroutines();
            StartCoroutine(InvincibilityRoutine(duration));
        }

        private System.Collections.IEnumerator InvincibilityRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            IsInvincible = false;
        }

        #region IProjectileHitHandler

        /// <summary>
        /// Bridges the projectile system into health: builds a <see cref="DamageArgs"/> from the hit
        /// context and returns whether the hit killed this target.
        /// </summary>
        public bool OnProjectileHit(ProjectileHitContext context)
        {
            if (!IsAlive) return false;

            var args = new DamageArgs
            {
                Damage = context.Damage,
                HitPoint = context.HitPoint,
                ChanceToHit = context.ChanceToHit,
                Instigator = context.Projectile != null && context.Projectile.Owner != null
                    ? context.Projectile.Owner.transform
                    : null,
            };

            Damage(args);
            return !IsAlive;
        }

        #endregion
    }
}
