using System;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Active abilities require manual activation. Passive abilities trigger automatically via their trigger components.
    /// </summary>
    public enum AbilityType { Passive, Active }

    /// <summary>
    /// Design-time configuration for one ability. Holds all components (triggers, conditions, effects, selector)
    /// that define the ability. Converted to a <see cref="RuntimeAbility"/> when added to a <see cref="CharacterAbilityModule"/>.
    ///
    /// Projects populate this from CSV, JSON, ScriptableObjects, or programmatically.
    /// The Effects list supports multiple effects per ability — simply add as many as needed.
    /// </summary>
    [Serializable]
    public class Ability
    {
        public int ID;
        public string Name;

        [TextArea]
        public string Description;

        /// <summary>Active or Passive. Active abilities can be manually triggered; passive abilities fire automatically.</summary>
        public AbilityType Type;

        /// <summary>Base cooldown in seconds. Scaled by the owner's attack speed and cooldown reduction stats.</summary>
        public float CooldownDuration;

        /// <summary>Tags that identify this ability. Used for prerequisite checks and filtering.</summary>
        public string[] Tags;

        /// <summary>Tags of abilities that must already be owned before this ability can be added.</summary>
        public string[] PrerequisiteAbilityTags;

        /// <summary>Stat groups this ability belongs to (e.g. "Attack", "Fever"). Used for stat modifier scoping.</summary>
        public string[] StatGroups;

        /// <summary>If false, only one instance of this ability ID can exist on a character.</summary>
        public bool AllowMultiple = true;

        /// <summary>Optional path to an animation/skill asset for this ability.</summary>
        public string ActionSkillDataPath;

        /// <summary>Whether the owner can move while this ability is casting.</summary>
        public bool CanMoveWhileCasting;

        /// <summary>The target selector configuration. Defines how targets are acquired.</summary>
        public AbilitySelector Selector;

        /// <summary>Triggers define WHEN the ability fires (e.g. on projectile hit, on timer).</summary>
        public List<AbilityTrigger> Triggers;

        /// <summary>Conditions define WHETHER the ability should fire (e.g. chance, health threshold).</summary>
        public List<AbilityCondition> Conditions;

        /// <summary>Effects define WHAT the ability does (e.g. shoot projectile, apply buff, modify stats).
        /// Multiple effects execute sequentially in list order.</summary>
        public List<AbilityEffect> Effects;

        public Ability()
        {
            StatGroups = Array.Empty<string>();
            Tags = Array.Empty<string>();
            PrerequisiteAbilityTags = Array.Empty<string>();
            Triggers = new List<AbilityTrigger>();
            Conditions = new List<AbilityCondition>();
            Effects = new List<AbilityEffect>();
        }
    }
}
