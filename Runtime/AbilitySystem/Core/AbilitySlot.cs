using System;
using UnityEngine;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// A named slot that can hold a single RuntimeAbility. Used to organize abilities
    /// by purpose (e.g. "Attack", "Passive", "Ultimate") and allow abilities to be
    /// swapped at runtime without changing referencing code.
    /// </summary>
    [Serializable]
    public class AbilitySlot
    {
        /// <summary>The slot identifier (e.g. "Attack", "Passive").</summary>
        public string ID;

        /// <summary>The ability currently in this slot.</summary>
        public RuntimeAbility RuntimeAbility;

        /// <summary>The module that owns this slot.</summary>
        public CharacterAbilityModule AbilityModule;

        public AbilitySlot(string id, CharacterAbilityModule module, RuntimeAbility runtimeAbility = null)
        {
            ID = id;
            AbilityModule = module;
            RuntimeAbility = runtimeAbility;
        }

        public void SetRuntimeAbility(RuntimeAbility ability)
        {
            RuntimeAbility = ability;
        }
    }
}
