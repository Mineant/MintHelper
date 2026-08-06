using UnityEngine;

namespace MioHelper.Samples.CharacterSystem
{
    /// <summary>
    /// Minimal 2D movement for a sample character: velocity interpolation toward a direction,
    /// knockback and stun. Drives the owner's Rigidbody2D. A <see cref="SampleCharacterBehaviour"/>,
    /// so it is wired up automatically by <see cref="SampleCharacter.Initialize"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class SampleMovement : SampleCharacterBehaviour
    {
        [Tooltip("Top movement speed in units/second.")]
        public float Speed = 5f;

        [Tooltip("Acceleration towards the target velocity.")]
        public float Acceleration = 20f;

        [Tooltip("Deceleration when there is no input.")]
        public float Deceleration = 20f;

        public bool ImmuneToStun;

        public Vector2 CurrentDirection { get; private set; }
        public Vector2 CurrentVelocity { get; private set; }

        /// <summary>Percent speed modifier: 1 = +100%, -1 = 50%.</summary>
        public float MovementSpeedModifier { get; protected set; }

        protected Rigidbody2D RigidBody;
        protected Vector2 _targetVelocity;
        protected Vector2 _knockbackVelocity;
        protected bool _isStunned;

        public override void Initialize(SampleCharacter owner)
        {
            base.Initialize(owner);
            RigidBody = GetComponent<Rigidbody2D>();
            SetMovement(Vector2.zero);
            _targetVelocity = Vector2.zero;
            _knockbackVelocity = Vector2.zero;
            MovementSpeedModifier = 0f;
            _isStunned = false;
        }

        protected virtual void FixedUpdate()
        {
            if (RigidBody == null) return;

            if (_isStunned)
            {
                RigidBody.velocity = Vector2.zero;
                return;
            }

            _targetVelocity = CurrentDirection * GetFinalSpeed();

            // Knockback decays.
            _knockbackVelocity *= 0.9f;
            if (_knockbackVelocity.sqrMagnitude < 0.001f) _knockbackVelocity = Vector2.zero;

            Vector2 target = _targetVelocity + _knockbackVelocity;
            float lerp = CurrentDirection.sqrMagnitude > 0.01f ? Acceleration : Deceleration;
            CurrentVelocity = lerp >= 999999f
                ? target
                : Vector2.MoveTowards(CurrentVelocity, target, lerp * Time.fixedDeltaTime);

            RigidBody.velocity = CurrentVelocity;
        }

        public virtual void SetMovement(Vector2 movement) => CurrentDirection = movement;

        public virtual void SetSpeed(float speed) => Speed = speed;

        public void ChangeSpeedModifier(float modifier) => MovementSpeedModifier += modifier;

        public float GetFinalSpeed()
        {
            if (MovementSpeedModifier >= 0f)
                return Speed * (1f + MovementSpeedModifier);
            return Speed * (1f / (1f - MovementSpeedModifier));
        }

        public void Knockback(Vector2 direction, float strength)
        {
            if (_isStunned) return;
            _knockbackVelocity = direction.normalized * strength;
        }

        public bool IsMoving() =>
            CurrentVelocity.sqrMagnitude > 0.01f
            || _targetVelocity.sqrMagnitude > 0.01f
            || _knockbackVelocity.sqrMagnitude > 0.01f;

        public void Stun(float duration)
        {
            if (ImmuneToStun) return;
            _isStunned = true;
            StopAllCoroutines();
            StartCoroutine(StunRoutine(duration));
        }

        private System.Collections.IEnumerator StunRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            _isStunned = false;
        }
    }
}
