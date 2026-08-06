using UnityEngine;
using MioHelper.ProjectileSystem;

namespace MioHelper.Samples.ProjectileSystem
{
    /// <summary>
    /// Self-contained demo of the projectile system: builds a projectile prototype and a target
    /// at runtime (no scene assets needed), then launches a spread fan toward the target on an
    /// interval. Attach to an empty GameObject in any scene and press Play.
    ///
    /// Shows both seams in action:
    ///   - the projectile's hit is consumed by <see cref="SampleProjectileHitHandler"/>
    ///     (the IProjectileHitHandler adapter), which owns health + death;
    ///   - pooling is visible via ProjectileSystem.Pool (no per-shot Instantiate in the loop).
    ///
    /// In a real project, the projectile prototype is a serialized prefab asset and the target
    /// is a designed entity — the framework doesn't care how either is authored.
    /// </summary>
    public class SampleProjectileBootstrap : MonoBehaviour
    {
        [Header("Demo")]

        [Tooltip("How often a spread fan is launched.")]
        public float LaunchInterval = 1f;

        [Tooltip("Projectiles per fan.")]
        public int ProjectileCount = 5;

        [Tooltip("Total fan angle in degrees.")]
        public float Spread = 45f;

        [Header("Projectile prototype configuration")]
        public float Speed = 15f;
        public float LifeTime = 3f;
        public float Damage = 25f;
        public int Pierce = 1;

        [Header("Target")]
        public float TargetMaxHealth = 100f;

        private Projectile _prototype;
        private Transform _target;
        private float _nextLaunchTime;

        private void Awake()
        {
            _prototype = BuildProjectilePrototype();
            _target = BuildTarget();
        }

        private void Update()
        {
            if (_prototype == null || _target == null) return;
            if (Time.time < _nextLaunchTime) return;

            _nextLaunchTime = Time.time + LaunchInterval;

            // Launch the fan toward the target. Direction is auto-resolved owner→target
            // because no explicit Direction is set; ProjectileSystem.Launch spreads the fan.
            var args = new ProjectileArgs
            {
                ProjectilePrefab = _prototype,
                Owner = gameObject,
                Target = _target,
                ProjectileCount = ProjectileCount,
                Spread = Spread,
                Damage = Damage,
                LifeTime = LifeTime,
                Pierce = Pierce,
            };
            ProjectileSystem.Launch(args);
        }

        private Projectile BuildProjectilePrototype()
        {
            var go = new GameObject("SampleProjectile Prototype");
            go.SetActive(false); // a prototype never runs logic in the scene

            // Visible: a magenta square so the demo is observable.
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeWhiteSquareSprite();
            sr.color = new Color(1f, 0.2f, 0.8f);
            sr.sortingOrder = 10;

            // Physics: kinematic body + trigger collider (the collision source is auto-added).
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            // The framework component. Everything not set here falls back to serialized defaults.
            var projectile = go.AddComponent<Projectile>();
            projectile.Speed = Speed;
            projectile.LifeTime = LifeTime;
            projectile.Damage = Damage;
            projectile.Pierce = Pierce;
            projectile.FaceMovement = true;
            projectile.TargetLayer = LayerMask.GetMask("Default");
            return projectile;
        }

        private Transform BuildTarget()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "SampleTarget";
            go.transform.position = new Vector3(0f, 3f, 0f);

            // 2D physics uses this trigger collider; the primitive's 3D BoxCollider is inert for it.
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            go.AddComponent<SampleProjectileHitHandler>().MaxHealth = TargetMaxHealth;
            return go.transform;
        }

        private static Sprite MakeWhiteSquareSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            // 1 pixel at 1 PPM → a 1×1-unit sprite, matching the 0.5-radius collider's diameter.
            return Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
