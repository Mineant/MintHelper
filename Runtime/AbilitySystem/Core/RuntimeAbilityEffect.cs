using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Runtime behavior for an ability effect. Controls the timed execution lifecycle:
    /// PreExecute (during delay) → Execute (when delay expires) → PostExecute (after duration).
    ///
    /// Uses time-based execution. Subclasses override Execute() to implement behavior.
    /// </summary>
    [Serializable]
    public class RuntimeAbilityEffect : RuntimeAbilityComponent<AbilityEffect>
    {
        [SerializeField] protected float _currentDelay;
        [SerializeField] protected float _currentDuration;
        [SerializeField] protected bool _isActivated;
        [SerializeField] protected bool _isWaiting;
        protected RuntimeAbilityEffectArgs _abilityArgs;

        /// <summary>
        /// Whether this effect has finished executing. Override to add custom completion conditions
        /// (e.g. wait for animation or projectile to land).
        /// </summary>
        public virtual bool IsExecutionFinished() => !_isWaiting;

        /// <summary>Override for the PreDelay phase.</summary>
        protected virtual void PreExecute(RuntimeAbilityEffectArgs abilityArgs, float currentDelay, float currentDuration) { }

        /// <summary>Override for the Execute phase (fires after the delay expires).</summary>
        protected virtual void Execute(RuntimeAbilityEffectArgs abilityArgs) { }

        /// <summary>Override for the PostExecute phase (fires after execute + duration).</summary>
        protected virtual void PostExecute(RuntimeAbilityEffectArgs abilityArgs) { }

        /// <summary>Called every frame while the effect is actively executing.</summary>
        protected virtual void OnExecutingUpdate(float deltaTime) { }

        /// <summary>Called after the RuntimeAbility has finished executing all effects, including this one.</summary>
        public virtual void OnAbilityFinished() { }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            if (_isActivated && _abilityArgs != null)
                OnExecutingUpdate(deltaTime);
        }

        /// <summary>Stop execution immediately.</summary>
        public void StopExecution()
        {
            _abilityArgs = null;
            _isWaiting = false;
            _isActivated = false;
        }

        /// <summary>
        /// Main coroutine for effect execution. Called by RuntimeAbility.
        /// Waits for the delay, fires Execute, waits for duration, then calls PostExecute.
        /// </summary>
        public IEnumerator ExecuteCoroutine(RuntimeAbilityEffectArgs abilityArgs)
        {
            _abilityArgs = abilityArgs;
            if (_abilityArgs == null) _abilityArgs = new RuntimeAbilityEffectArgs();

            _isActivated = true;
            _isWaiting = true;

            _currentDelay = GetScaledDelay();
            _currentDuration = GetScaledDuration();

            PreExecute(_abilityArgs, _currentDelay, _currentDuration);

            // Wait for delay
            while (_currentDelay > 0f)
            {
                _currentDelay -= Time.deltaTime;
                yield return null;
            }

            Execute(_abilityArgs);

            // Wait for duration
            while (_currentDuration > 0f)
            {
                _currentDuration -= Time.deltaTime;
                yield return null;
            }

            _isWaiting = false;

            // Wait for custom completion conditions
            while (!IsExecutionFinished())
            {
                yield return null;
            }

            PostExecute(_abilityArgs);

            _isActivated = false;
            _abilityArgs = null;
        }

        /// <summary>Get the effect delay (unscaled). Override for custom behavior.</summary>
        public virtual float GetDelay() => OwnerAbilityComponent?.Delay ?? 0f;

        /// <summary>Get the delay scaled by attack speed.</summary>
        public virtual float GetScaledDelay()
        {
            float attackSpeed = _abilityArgs?.AttackSpeed ?? 1f;
            attackSpeed = Mathf.Max(0.1f, attackSpeed);
            return GetDelay() / attackSpeed;
        }

        /// <summary>Get the effect duration (unscaled). Override for custom behavior.</summary>
        public virtual float GetDuration() => OwnerAbilityComponent?.Duration ?? 0f;

        /// <summary>Get the duration scaled by attack speed.</summary>
        public virtual float GetScaledDuration()
        {
            float attackSpeed = _abilityArgs?.AttackSpeed ?? 1f;
            attackSpeed = Mathf.Max(0.1f, attackSpeed);
            return GetDuration() / attackSpeed;
        }
    }
}
