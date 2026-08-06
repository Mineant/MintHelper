using System;
using UnityEngine;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Cooldown management for a runtime ability. Tracks base duration, current remaining time,
    /// and supports cooldown reduction via stats and percentage-based instant reduction.
    /// </summary>
    [Serializable]
    public class RuntimeAbilityCooldownComponent : RuntimeAbilityComponent
    {
        [SerializeField] private float _baseCooldownDuration;
        [SerializeField] private float _currentCooldown;

        public bool IsOnCooldown => _currentCooldown > 0f;

        public override async Cysharp.Threading.Tasks.UniTask InitAsync(AbilityComponent ownerAbilityComponent, RuntimeAbility ownerRuntimeAbility)
        {
            await base.InitAsync(ownerAbilityComponent, ownerRuntimeAbility);
            _baseCooldownDuration = ownerRuntimeAbility?.OwnerAbility?.CooldownDuration ?? 0f;
            _currentCooldown = 0f;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!IsOnCooldown) return;
            _currentCooldown = Mathf.Max(0f, _currentCooldown - deltaTime);
        }

        public bool CanUseAbility()
        {
            if (_baseCooldownDuration <= 0f) return true;
            return !IsOnCooldown;
        }

        public void StartCooldown()
        {
            _currentCooldown = GetMaxCooldown();
        }

        /// <summary>
        /// Get the maximum cooldown value. Override in a subclass to integrate with stat systems
        /// (e.g. apply cooldown reduction and attack speed scaling).
        /// </summary>
        public virtual float GetMaxCooldown()
        {
            return _baseCooldownDuration;
        }

        public float GetCurrentCooldown() => _currentCooldown;

        /// <summary>Immediately reduce remaining cooldown by a percentage (0–1).</summary>
        public void ReduceCurrentCooldownByPercent(float percent)
        {
            if (!IsOnCooldown) return;
            _currentCooldown = Mathf.Max(0f, _currentCooldown - _currentCooldown * Mathf.Clamp01(percent));
        }
    }
}
