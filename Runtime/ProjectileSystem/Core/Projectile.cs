using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MioHelper.AbilitySystem;

namespace MioHelper.ProjectileSystem
{
    /// <summary>
    /// The deep projectile MonoBehaviour. Owns the full lifecycle — movement, tracking,
    /// collision, pierce/bounce, sustained damage, despawn — behind a narrow interface:
    /// <see cref="Initialize(ProjectileArgs)"/> then <see cref="Launch"/>. All per-shot state
    /// lives in <c>_current*</c> fields that <see cref="Initialize"/> resets from
    /// args-with-prefab-fallback, so the object pool never leaks state between uses.
    ///
    /// What this class does NOT know (by design, Candidate B): how damage is applied, knockback,
    /// elements, buffs, hit feedback, danger zones, or any concrete entity type. On impact it
    /// emits a <see cref="ProjectileHitContext"/> into an <see cref="IProjectileHitHandler"/> the
    /// consumer implements, and fires events through <see cref="ProjectileEventBus"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class Projectile : MonoBehaviour, ISelectable
    {
        #region Serialized Configuration

        [Header("Identity")]
        [Tooltip("This projectile's tag. Added to Tags automatically. Does not need to be unique.")]
        public string Id;

        [Tooltip("Other tags. Duplicates are filtered when Tags is built in Awake.")]
        [SerializeField] private List<string> _tags;

        [Tooltip("The entity that launched this projectile. Its own collider is ignored.")]
        public GameObject Owner;

        [Header("Movement")]
        public float StartScale = 1f;
        public float Speed;
        public float MinSpeed;
        public float MaxSpeed;
        public float Acceleration;
        public bool FaceMovement;

        [Tooltip("Per-second spiral turn. 0 = no spiral.")]
        public float SpiralMovement;

        [Tooltip("How quickly the spiral decays over the projectile's life.")]
        public float SpiralDecayRate = 1f;

        [Header("Lifetime & Damage")]
        public float LifeTime;
        public float Damage;

        [Tooltip("Probability this hit is applied. The handler decides how to consume it.")]
        [Range(0f, 1f)]
        public float ChanceToHit = 1f;

        [Tooltip("If true, this projectile's hit should not trigger target invincibility.")]
        public bool ShouldNotTriggerInvincibility;

        [Tooltip("Generic impact strength; a hit handler may use it for knockback/impulse.")]
        public float KnockbackStrength;

        [Header("Collision")]
        public LayerMask TargetLayer;
        public LayerMask ObstacleLayer;

        [Tooltip("Seconds of obstacle invincibility after launch (avoid hitting the wall you just fired from).")]
        public float InvincibleOnShoot = 0.1f;

        [Tooltip("Seconds after launch before damage to targets is enabled.")]
        public float DamageDelayOnShoot;

        [Tooltip("If true, on death the collider is disabled and the projectile despawns 2s later.")]
        public bool DisableCollisionOnDeath;

        [Header("Pierce")]
        public bool PierceAll;

        [Tooltip("Number of extra targets this projectile passes through.")]
        public int Pierce;

        [Header("Bounce")]
        public bool CanBounce;
        public int BounceCount;
        public float BounceDetectRadius;
        public bool CanBounceOnObstacle;
        public bool CanBounceOnTarget = true;

        [Header("Tracking")]
        public bool AutoTrackTarget;
        public float LerpSpeed;
        public float TrackingRadius;
        public float TrackDelay;

        [Tooltip("How often the projectile re-selects its closest target.")]
        public float TrackInterval = 1f;

        [Header("Multi-Hit")]
        public bool EnableMultiHit;

        [Tooltip("A target can be hit again after this many seconds.")]
        public float MultiHitResetTargetDuration;

        [Header("Sustained Damage")]
        public bool EnableSustainedDamage;

        [Tooltip("Periodically re-allows targets to be hit.")]
        public float SustainedDamageInterval;

        [Header("Effects")]
        public GameObject OnSpawnParticle;
        public GameObject OnHitParticle;

        #endregion

        #region Runtime State

        /// <summary>Tags this projectile routes events through (built in Awake from Id + _tags).</summary>
        public HashSet<string> Tags { get; protected set; }

        /// <summary>The prefab this instance was spawned from (used by the pool to key stacks).</summary>
        public Projectile PrefabSource { get; set; }

        public bool Launched => _launched;
        public bool IsDead => _isDead;
        public Vector3 CurrentDirection => _currentDirection;
        public float CurrentSpeed => _currentSpeed;
        public float CurrentDamage => _currentDamage;
        public float CurrentChanceToHit => _currentChanceToHit;
        public float CurrentKnockbackStrength => _currentKnockbackStrength;

        public bool IsInInvincibilityPeriod => _launched && (Time.time - _launchTimestamp) < InvincibleOnShoot;
        public bool CanDealDamage => _launched && (Time.time - _launchTimestamp) >= Mathf.Max(0f, DamageDelayOnShoot);

        protected Vector3 _currentDirection = Vector3.right;
        protected float _currentSpeed;
        protected int _currentPierce;
        protected int _currentBounce;
        protected int _bouncesPerformed;
        protected float _bounceDamageIncrease;
        protected float _currentLifeTime;
        protected float _aliveDuration;
        protected float _currentDamage;
        protected float _currentChanceToHit;
        protected float _currentKnockbackStrength;

        protected Transform _currentTarget;
        protected HashSet<GameObject> _hitObjects;
        protected bool _launched;
        protected bool _isDead;
        protected bool _isDying;
        protected float _launchTimestamp;
        protected float _lastTrackTimestamp;
        protected float _lastBounceTimestamp;
        protected float _sustainedDamageRefreshTimestamp;

        protected Rigidbody2D _rigidBody;
        protected Collider2D _collider;
        protected CollisionDetectionComponent _collisionDetection;

        /// <summary>Fires when the projectile is launched.</summary>
        public event System.Action OnProjectileShoot;

        /// <summary>Fires when the projectile deals damage to a target. Argument = the target GameObject.</summary>
        public event System.Action<GameObject> OnDealDamage;

        #endregion

        #region ISelectable

        public bool IsValidTarget => true;
        public Transform Transform => transform;
        public Vector3 GetHitPosition() => transform.position;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            _rigidBody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _hitObjects = new HashSet<GameObject>();

            Tags = new HashSet<string>();
            if (_tags != null)
                foreach (var tag in _tags)
                    if (!string.IsNullOrEmpty(tag)) Tags.Add(tag);
            if (!string.IsNullOrEmpty(Id)) Tags.Add(Id);

            _collisionDetection = GetComponent<CollisionDetectionComponent>();
            if (_collisionDetection == null)
                _collisionDetection = gameObject.AddComponent<UnityColliderCollisionDetection>();
        }

        protected virtual void OnEnable()
        {
            if (_collisionDetection == null) return;
            _collisionDetection.OnTriggerEnter2D += OnTriggerEnter2DEvent;
            _collisionDetection.OnTriggerStay2D += OnTriggerStay2DEvent;
        }

        protected virtual void OnDisable()
        {
            if (_collisionDetection == null) return;
            _collisionDetection.OnTriggerEnter2D -= OnTriggerEnter2DEvent;
            _collisionDetection.OnTriggerStay2D -= OnTriggerStay2DEvent;
        }

        protected virtual void Update()
        {
            if (!_launched || _isDead) return;

            _currentLifeTime -= Time.deltaTime;
            _aliveDuration += Time.deltaTime;

            if (_currentLifeTime <= 0f)
                Destroy();
        }

        protected virtual void FixedUpdate()
        {
            if (!_launched) return;

            ProcessTracking();
            Movement();
            ProcessSustainDamage();
        }

        #endregion

        #region Lifecycle (initialize → launch → destroy)

        /// <summary>
        /// Resets all per-shot state from args-with-prefab-fallback. This is the pool-safe
        /// reset: every spawn calls it before Launch, so no state leaks between uses and the
        /// prefab's serialized defaults are never mutated.
        /// </summary>
        public virtual void Initialize(ProjectileArgs args)
        {
            Owner = args.Owner != null ? args.Owner : gameObject;

            _currentDirection = Vector3.right;
            _currentTarget = null;
            _launched = false;
            _isDead = false;
            _isDying = false;
            _bouncesPerformed = 0;
            _aliveDuration = 0f;
            _lastTrackTimestamp = float.NegativeInfinity;
            _lastBounceTimestamp = float.NegativeInfinity;
            _sustainedDamageRefreshTimestamp = 0f;

            _currentSpeed = Speed;
            _currentPierce = args.Pierce > 0 ? args.Pierce : Pierce;
            _currentBounce = args.BounceCount > 0 ? args.BounceCount : BounceCount;
            _currentLifeTime = args.LifeTime > 0 ? args.LifeTime : LifeTime;
            _currentDamage = args.Damage > 0 ? args.Damage : Damage;
            _currentChanceToHit = args.ChanceToHit > 0 ? args.ChanceToHit : ChanceToHit;
            _currentKnockbackStrength = args.KnockbackStrength > 0 ? args.KnockbackStrength : KnockbackStrength;
            _bounceDamageIncrease = args.BounceDamageIncrease;

            float scale = args.Size > 0f ? args.Size : (StartScale > 0f ? StartScale : 1f);
            transform.localScale = Vector3.one * scale;

            ResetHitObjects();
        }

        /// <summary>Fire the projectile. Must be preceded by <see cref="Initialize(ProjectileArgs)"/>.</summary>
        public virtual void Launch()
        {
            if (OnSpawnParticle != null)
                ProjectileSystem.SpawnParticle(OnSpawnParticle, transform.position, 5f);

            if (_collider != null) _collider.enabled = true;

            _launchTimestamp = Time.time;
            _sustainedDamageRefreshTimestamp = Time.time;
            _launched = true;

            ProjectileEventBus.Trigger(this, new ProjectileLaunchedEventArgs { Projectile = this });
            OnProjectileShoot?.Invoke();
        }

        /// <summary>Begin death. With DisableCollisionOnDeath, waits 2s before despawn.</summary>
        public virtual void Destroy()
        {
            if (_isDying) return;

            _isDying = true;
            _isDead = true;
            _launched = false;

            if (DisableCollisionOnDeath)
            {
                if (_collider != null) _collider.enabled = false;
                StartCoroutine(FinishDelayDestroy(2f));
            }
            else
            {
                DestroyImmediate();
            }
        }

        /// <summary>Despawn immediately (returns to the pool) and fire the destroyed event.</summary>
        public virtual void DestroyImmediate()
        {
            // Guard against double-destroy: block only when death was started elsewhere AND this
            // isn't the delayed-finish path (which clears _isDying just before calling here).
            if (_isDead && !_isDying) return;

            _isDead = true;
            _launched = false;
            ProjectileSystem.UnregisterActiveProjectile(this);
            ProjectileSystem.Pool.Despawn(this);
            ProjectileEventBus.Trigger(this, new ProjectileDestroyedEventArgs { Projectile = this });
        }

        private IEnumerator FinishDelayDestroy(float duration)
        {
            yield return new WaitForSeconds(duration);
            _isDying = false;
            DestroyImmediate();
        }

        #endregion

        #region Public Setters (used by the stat provider seam and custom effects)

        public virtual void SetDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.0001f)
                _currentDirection = direction.normalized;
            if (FaceMovement) FaceCurrentDirection();
        }

        public virtual void SetTrackTarget(Transform target) => _currentTarget = target;

        public virtual void SetCurrentLifetime(float lifetime) => _currentLifeTime = lifetime;

        public virtual void SetDamage(float damage) => _currentDamage = damage;

        public virtual void SetPierce(int pierce) => _currentPierce = pierce;

        public virtual void SetBounceCount(int bounce) => _currentBounce = bounce;

        public virtual void SetSpeed(float speed) => _currentSpeed = speed;

        public virtual void SetChanceToHit(float chanceToHit) => _currentChanceToHit = chanceToHit;

        public virtual void SetKnockbackStrength(float strength) => _currentKnockbackStrength = strength;

        public virtual void SetSize(float size)
        {
            if (size <= 0f) return;
            transform.localScale = Vector3.one * size;
        }

        public virtual void ResetHitObjects() => _hitObjects.Clear();

        public virtual void AddHitObject(GameObject obj)
        {
            if (obj != null) _hitObjects.Add(obj);
        }

        public virtual void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
            Tags.Add(tag);
        }

        public bool CheckIsObstacleLayer(int layer)
        {
            if (layer < 0) return false;
            return (ObstacleLayer.value & (1 << layer)) != 0;
        }

        #endregion

        #region Movement & Tracking

        protected virtual void ProcessSustainDamage()
        {
            if (!_launched || !EnableSustainedDamage) return;
            if (_sustainedDamageRefreshTimestamp + SustainedDamageInterval > Time.time) return;

            _sustainedDamageRefreshTimestamp = Time.time;
            ResetHitObjects();
        }

        protected virtual void ProcessTracking()
        {
            if (!AutoTrackTarget || !_launched) return;
            if (_launchTimestamp + TrackDelay > Time.time) return;

            // Pause tracking briefly after a bounce so the projectile leaves the old target.
            if (Time.time - _lastBounceTimestamp < 0.2f) return;

            if (_currentTarget != null)
            {
                if (!_currentTarget.gameObject.activeSelf || _hitObjects.Contains(_currentTarget.gameObject))
                {
                    SetTrackTarget(null);
                }
                else
                {
                    SetDirectionToTarget();
                    return;
                }
            }
            else if (Time.time - _lastTrackTimestamp < TrackInterval)
            {
                return;
            }

            _lastTrackTimestamp = Time.time;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, TrackingRadius, TargetLayer);
            Collider2D closest = null;
            float closestDistance = float.MaxValue;
            foreach (var collider in colliders)
            {
                if (_hitObjects.Contains(collider.gameObject)) continue;

                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closest = collider;
                    closestDistance = distance;
                }
            }

            if (closest != null)
            {
                _currentTarget = closest.transform;
                SetDirectionToTarget();
            }
            else
            {
                _currentTarget = null;
            }
        }

        protected virtual void SetDirectionToTarget()
        {
            if (_currentTarget == null) return;

            Vector3 direction = _currentTarget.position - transform.position;
            if (AutoTrackTarget)
                direction = Vector3.MoveTowards(_currentDirection, direction, LerpSpeed * Time.deltaTime);
            SetDirection(direction);
        }

        public virtual void Movement()
        {
            // Spiral: turn the direction over time, decaying via SpiralDecayRate.
            if (Mathf.Abs(SpiralMovement) > 0f)
            {
                float decayFactor = 1f / (1f + SpiralDecayRate * _aliveDuration);
                float rotationSpeed = (SpiralMovement / 100f) * 360f * decayFactor * Time.deltaTime;
                _currentDirection = Quaternion.Euler(0, 0, rotationSpeed) * _currentDirection;
            }

            Vector3 movement = _currentDirection * (_currentSpeed / 10f) * Time.deltaTime;
            if (_rigidBody != null)
            {
                _rigidBody.MovePosition(transform.position + movement);
            }
            else
            {
                transform.position += movement;
            }

            _currentSpeed += Acceleration * Time.deltaTime;
            if (_currentSpeed < MinSpeed) _currentSpeed = MinSpeed;
            if (MaxSpeed > 0f && _currentSpeed > MaxSpeed) _currentSpeed = MaxSpeed;

            if (FaceMovement) FaceCurrentDirection();
        }

        private void FaceCurrentDirection()
        {
            Vector3 faceDirection = _currentDirection;
            if (_currentSpeed < 0f) faceDirection = -faceDirection;
            transform.right = faceDirection;
        }

        #endregion

        #region Collision

        protected virtual void OnTriggerEnter2DEvent(Collider2D other) => CollisionCheck(other);
        protected virtual void OnTriggerStay2DEvent(Collider2D other) => CollisionCheck(other);

        protected virtual void CollisionCheck(Collider2D collider)
        {
            if (collider == null) return;
            if (Owner != null && collider.gameObject == Owner) return;
            if (_hitObjects.Contains(collider.gameObject)) return;

            bool isObstacle = CheckIsObstacleLayer(collider.gameObject.layer);
            bool isTarget = (TargetLayer.value & (1 << collider.gameObject.layer)) != 0;
            if (!isObstacle && !isTarget) return;

            // Invincibility only suppresses obstacles (so you don't hit the wall you fired from).
            if (isObstacle && IsInInvincibilityPeriod) return;
            if (isTarget && !CanDealDamage) return;

            // Target hit: hand the outcome decision to the consumer's hit handler.
            // A collider can sit on both layers; a target still takes the hit.
            if (isTarget && collider.TryGetComponent(out IProjectileHitHandler hitHandler))
                ProcessTargetHit(hitHandler, collider);

            // The framework owns pierce/bounce; it runs regardless of what the handler decided.
            ProcessBounceAndPierce(isObstacle);

            if (OnHitParticle != null && (isTarget || !PierceAll))
                ProjectileSystem.SpawnParticle(OnHitParticle, transform.position, 2f);
        }

        protected virtual void ProcessTargetHit(IProjectileHitHandler handler, Collider2D collider)
        {
            float damage = CalculateDamage();
            bool killed = handler.OnProjectileHit(new ProjectileHitContext(this, collider, damage, _currentChanceToHit, transform.position));

            OnDealDamage?.Invoke(collider.gameObject);

            _hitObjects.Add(collider.gameObject);
            ProjectileEventBus.Trigger(this, new ProjectileHitEventArgs { Projectile = this, HitObject = collider.gameObject });

            if (EnableMultiHit && MultiHitResetTargetDuration > 0f && gameObject.activeInHierarchy)
                StartCoroutine(ResetHitTargetAfterDelay(collider.gameObject, MultiHitResetTargetDuration));

            if (killed)
                ProjectileEventBus.Trigger(this, new ProjectileKilledEventArgs { Projectile = this, KilledObject = collider.gameObject });
        }

        /// <summary>
        /// The damage arithmetic the core actually owns: base damage × bounce stacking.
        /// Everything else (modifiers, buffs, elements) is the hit handler's business.
        /// </summary>
        protected virtual float CalculateDamage()
        {
            float damage = _currentDamage;
            if (_bouncesPerformed > 0 && _bounceDamageIncrease > 0f)
                damage *= 1f + _bouncesPerformed * _bounceDamageIncrease;
            return damage;
        }

        private IEnumerator ResetHitTargetAfterDelay(GameObject target, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (target != null && _hitObjects.Contains(target))
                _hitObjects.Remove(target);
        }

        #endregion

        #region Bounce & Pierce

        protected virtual void ProcessBounceAndPierce(bool isObstacle)
        {
            if (CanBounce && _currentBounce > 0 && (CanBounceOnTarget || CanBounceOnObstacle))
            {
                if (!HandleBounce(isObstacle))
                    HandlePierce();
            }
            else
            {
                HandlePierce();
            }
        }

        protected virtual bool HandleBounce(bool isObstacle)
        {
            _currentBounce -= 1;

            if (isObstacle && CanBounceOnObstacle)
            {
                BounceOffObstacle();
                _lastBounceTimestamp = Time.time;
                _bouncesPerformed++;
                return true;
            }

            if (!isObstacle && CanBounceOnTarget)
            {
                BounceOffTarget();
                _lastBounceTimestamp = Time.time;
                _bouncesPerformed++;
                return true;
            }

            if (_currentBounce <= 0)
                Destroy();

            return false;
        }

        protected virtual void BounceOffObstacle()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, _currentDirection, 10f, ObstacleLayer);
            if (hit.collider != null)
                SetDirection(Vector2.Reflect(_currentDirection, hit.normal));
        }

        protected virtual void BounceOffTarget()
        {
            Transform newTarget = FindBounceTarget();
            Vector2 direction = newTarget != null
                ? (newTarget.position - transform.position).normalized
                : Random.insideUnitCircle.normalized;
            SetDirection(direction);
        }

        protected virtual void HandlePierce()
        {
            _currentPierce -= 1;
            if (!PierceAll && _currentPierce <= 0)
                Destroy();
        }

        public virtual Transform FindBounceTarget()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, BounceDetectRadius, TargetLayer);
            foreach (var hit in hits)
            {
                if (!_hitObjects.Contains(hit.gameObject))
                    return hit.transform;
            }
            return null;
        }

        #endregion
    }
}
