using UnityEngine;

namespace MioHelper.ProjectileSystem
{
    /// <summary>
    /// Default collision source: forwards Unity's 2D trigger callbacks onto the
    /// <see cref="CollisionDetectionComponent"/> events. Auto-added to a projectile in Awake
    /// when no other CollisionDetectionComponent is present.
    /// </summary>
    public class UnityColliderCollisionDetection : CollisionDetectionComponent
    {
        private void OnTriggerEnter2D(Collider2D collision) => RaiseEnter(collision);
        private void OnTriggerStay2D(Collider2D collision) => RaiseStay(collision);
        private void OnTriggerExit2D(Collider2D collision) => RaiseExit(collision);
    }
}
