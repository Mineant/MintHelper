using System;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Interface that allows ability-owning entities (characters, monsters, NPCs, etc.)
    /// to be referenced without coupling the ability system to a specific entity class.
    /// Projects implement this on their own entity types.
    /// </summary>
    public interface IAbilityOwner
    {
        /// <summary>Unique identifier for this entity (used for buff providers, kill tracking, etc.).</summary>
        string ID { get; }

        /// <summary>
        /// Returns a behaviour component of type T attached to the same GameObject.
        /// This avoids the ability system needing to know about specific component types.
        /// </summary>
        T GetBehaviour<T>();

        /// <summary>The GameObject this owner is on.</summary>
        UnityEngine.GameObject GameObject { get; }
    }
}
