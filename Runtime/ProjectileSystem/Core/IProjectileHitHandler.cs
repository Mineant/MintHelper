using UnityEngine;

namespace MioHelper.ProjectileSystem
{
    /// <summary>
    /// The hit seam (Candidate B): a consumer-implemented contract that decides what happens
    /// when a projectile hits something. The framework owns the arithmetic it actually computes
    /// — movement, tracking, pierce, bounce, damage stacking — and hands the *outcome* decision
    /// to this handler.
    ///
    /// Projects implement this on their damageable entities (e.g. EC's Health) or on a dedicated
    /// adapter component. The framework never references a concrete Health type.
    ///
    /// One adapter = hypothetical seam, two = real. EC is adapter #1; the Sample Projectile
    /// Hit Handler in Samples~ is adapter #2.
    /// </summary>
    public interface IProjectileHitHandler
    {
        /// <summary>
        /// A projectile hit this object. Return true if the hit killed/destroyed the target
        /// (the framework uses the result to fire ProjectileKilled events).
        /// </summary>
        bool OnProjectileHit(ProjectileHitContext context);
    }

    /// <summary>
    /// Immutable payload handed to <see cref="IProjectileHitHandler"/>. Carries the resolved
    /// damage and the references the handler needs to decide the outcome (apply damage,
    /// knockback, element reactions, buffs, death, damage numbers — all consumer concerns).
    /// </summary>
    public readonly struct ProjectileHitContext
    {
        public readonly Projectile Projectile;
        public readonly Collider2D Collider;
        public readonly GameObject Target;
        public readonly float Damage;
        public readonly float ChanceToHit;
        public readonly Vector2 HitPoint;

        public ProjectileHitContext(Projectile projectile, Collider2D collider, float damage, float chanceToHit, Vector2 hitPoint)
        {
            Projectile = projectile;
            Collider = collider;
            Target = collider != null ? collider.gameObject : null;
            Damage = damage;
            ChanceToHit = chanceToHit;
            HitPoint = hitPoint;
        }
    }

    /// <summary>
    /// Optional seam for projects that resolve projectile stats from their own stat system
    /// (e.g. EC's CharacterStatTableQuery). Invoked once per launched projectile, after the
    /// core has applied args-with-prefab fallbacks; implementers override values through the
    /// projectile's public setters. The framework owns no stat vocabulary.
    /// </summary>
    public interface IProjectileStatProvider
    {
        void Apply(ProjectileArgs args, Projectile projectile);
    }

    /// <summary>
    /// Provides the base attack power used to compute projectile damage
    /// (damage = AttackPower × damage percentage). Projects implement this on their
    /// character / ability-owner entity; the sample provides a trivial MonoBehaviour.
    /// </summary>
    public interface IProjectileAttackSource
    {
        float AttackPower { get; }
    }
}
