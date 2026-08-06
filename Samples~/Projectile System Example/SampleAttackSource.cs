using UnityEngine;
using MioHelper.ProjectileSystem;

namespace MioHelper.Samples.ProjectileSystem
{
    /// <summary>
    /// Sample implementation of <see cref="IProjectileAttackSource"/> — the "attack power"
    /// seam the projectile system uses to compute damage (damage = AttackPower × percentage).
    /// In a real project, this lives on the character/ability-owner entity and reads from the
    /// project's stat system instead of a serialized float.
    /// </summary>
    public class SampleAttackSource : MonoBehaviour, IProjectileAttackSource
    {
        [field: SerializeField] public float AttackPower { get; set; } = 10f;
    }
}
