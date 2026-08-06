using UnityEngine;

namespace MioHelper.Samples.CharacterSystem
{
    /// <summary>
    /// Payload for <see cref="SampleHealth.Damage"/>. Build it from projectiles, melee swings,
    /// hazards, etc.
    /// </summary>
    [System.Serializable]
    public class DamageArgs
    {
        public float Damage;
        public Vector3 HitPoint;
        public Transform Instigator;
        public float ChanceToHit = 1f;
        public float CritChance;
        public float CritDamageMultiplier = 1.5f;
        public bool IgnoreInvincibility;

        public DamageArgs() { }

        public DamageArgs(float damage, Transform instigator = null, Vector3 hitPoint = default)
        {
            Damage = damage;
            Instigator = instigator;
            HitPoint = hitPoint;
        }
    }
}
