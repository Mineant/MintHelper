using System.Collections.Generic;
using MioHelper.ProjectileSystem;
using MioHelper.StatSystem;
using UnityEngine;

namespace MioHelper.Samples.CharacterSystem
{
    /// <summary>
    /// Self-bootstrapping demo of the Character Example sample (no scene assets needed). Builds a
    /// shooter and a target SampleCharacter at runtime, gives the shooter an ATTACK_POWER stat,
    /// then launches projectiles at the target until it dies. Attach to an empty GameObject and
    /// press Play.
    ///
    /// Shows the composition: SampleCharacter (IAbilityOwner / ISelectable / ITeamMember) +
    /// MioStatSheet + CharacterAbilityModule + SampleHealth (IProjectileHitHandler) + SampleMovement.
    /// </summary>
    public class SampleCharacterBootstrap : MonoBehaviour
    {
        [Header("Demo")]
        [Tooltip("How often a shot is fired.")]
        public float LaunchInterval = 0.8f;

        [Tooltip("Projectiles per shot.")]
        public int ProjectileCount = 1;

        [Tooltip("Total fan angle in degrees.")]
        public float Spread = 0f;

        [Header("Shooter")]
        public float ShooterMaxHealth = 100f;

        [Header("Projectile prototype configuration")]
        public float Speed = 15f;
        public float LifeTime = 3f;
        public float Damage = 25f;
        public int Pierce = 0;

        [Header("Target")]
        public float TargetMaxHealth = 100f;

        private Projectile _prototype;
        private SampleCharacter _shooter;
        private SampleCharacter _target;
        private float _nextLaunchTime;

        private static readonly object StatSource = new object();

        private void Awake()
        {
            _prototype = BuildProjectilePrototype();
            _shooter = BuildShooter();
            _target = BuildTarget();
        }

        private void Update()
        {
            if (_prototype == null || _shooter == null || _target == null) return;
            if (!_target.Health.IsAlive) return; // stop firing once the target dies
            if (Time.time < _nextLaunchTime) return;

            _nextLaunchTime = Time.time + LaunchInterval;

            ProjectileSystem.Launch(new ProjectileArgs
            {
                ProjectilePrefab = _prototype,
                Owner = _shooter.gameObject,
                Target = _target.Transform,
                ProjectileCount = ProjectileCount,
                Spread = Spread,
                Damage = Damage,
                LifeTime = LifeTime,
                Pierce = Pierce,
            });
        }

        private SampleCharacter BuildShooter()
        {
            var character = BuildCharacter("SampleShooter", ShooterMaxHealth, false);

            // Give the shooter an ATTACK_POWER stat, then read it back through the sheet.
            character.StatTable.Apply(new MioStatModifierTable
            {
                ModifierLists = new List<MioStatModifierGroup>
                {
                    new MioStatModifierGroup
                    {
                        Group = "Character",
                        Modifiers = new List<MioStatModifier>
                        {
                            new MioStatModifier { Stat = "ATTACK_POWER", Value = 50f, Type = MioStatModType.Flat },
                        },
                    },
                },
            }, StatSource);

            float attackPower = character.StatTable.GetTotalStatValue("ATTACK_POWER", new[] { "Character" }, true, 0f);
            Debug.Log($"[Character Example] SampleShooter ATTACK_POWER = {attackPower}");

            return character;
        }

        private SampleCharacter BuildTarget()
        {
            var character = BuildCharacter("SampleTargetCharacter", TargetMaxHealth, true);
            character.SetTeam(2);
            character.Health.OnDeath += () => Debug.Log("[Character Example] Target died.");
            character.transform.position = new Vector3(0f, 3f, 0f);
            return character;
        }

        private SampleCharacter BuildCharacter(string name, float maxHealth, bool despawnOnDeath)
        {
            var go = new GameObject(name);
            go.SetActive(false); // add/configure before any Awake runs

            // Physics entity: kinematic body + trigger collider. The projectile hit handler and the
            // collider must share the GameObject — Projectile finds it via collider.TryGetComponent.
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            var character = go.AddComponent<SampleCharacter>();
            var health = go.AddComponent<SampleHealth>();
            health.MaxHealth = maxHealth;
            health.DespawnOnDeath = despawnOnDeath;
            var movement = go.AddComponent<SampleMovement>();

            character.Initialize(); // wire while inactive so Health/stat sheet exist for Awake

            go.SetActive(true);
            return character;
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
