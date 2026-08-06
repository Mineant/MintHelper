MioHelper Projectile System Example
===================================

A self-contained demo of the projectile system. No scene assets needed.

Setup
-----
1. Open any empty scene.
2. Create an empty GameObject.
3. Add the SampleProjectileBootstrap component.
4. Press Play.

What you should see
-------------------
Every LaunchInterval seconds, a fan of magenta projectiles flies from the
bootstrap GameObject toward the cube target. Each hit subtracts Damage from the
target's health (SampleProjectileHitHandler); when health hits zero the target is
destroyed and the framework fires the ProjectileKilled event (see
ProjectileEventBus). Projectiles despawn on lifetime expiry or after piercing
Pierce targets, and return to the pool rather than being instantiated/destroyed.

How the seams show up
---------------------
- ProjectileSystem.Launch builds everything from a ProjectileArgs (no calls into
  a concrete pool, stat, or damage type).
- SampleProjectileHitHandler implements IProjectileHitHandler — the hit seam. The
  framework decides movement/tracking/pierce/bounce; the handler decides health,
  damage numbers, and death. This is adapter #2 (Element Conquest's Health-based
  handler is #1), which is what makes the seam real.
- The projectile prototype here is built in code for the demo. In a real project
  it would be a serialized prefab referenced from a ProjectilePresetData asset.
