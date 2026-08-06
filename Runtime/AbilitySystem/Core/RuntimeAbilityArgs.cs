using System;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Arguments passed to a trigger when it fires. Carries information about what triggered it
    /// (targets, aim position/direction, energy spent).
    /// </summary>
    [Serializable]
    public class RuntimeAbilityTriggerArgs
    {
        public List<ISelectable> Targets;
        public Vector3 AimPosition = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        public Vector3 AimDirection = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        public float EnergySpent;

        public RuntimeAbilityTriggerArgs()
        {
            Targets = new List<ISelectable>();
        }

        public RuntimeAbilityTriggerArgs(List<ISelectable> targets) : this()
        {
            Targets = targets;
        }
    }

    /// <summary>
    /// Arguments passed to each effect during execution. Built from trigger args
    /// plus runtime state (context, attack speed, animation controller).
    /// </summary>
    [Serializable]
    public class RuntimeAbilityEffectArgs
    {
        public List<ISelectable> Targets;
        public Vector3 AimPosition = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        public Vector3 AimDirection = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        public RuntimeAbilityContext Context;
        public float AttackSpeed = 1f;
        public float EnergySpent;

        /// <summary>Optionally, the animation/skill controller for frame-synced effects. Null if unused.</summary>
        public object ActionSkillController;

        /// <summary>Copy data from trigger args into this effect args.</summary>
        public void Init(RuntimeAbilityTriggerArgs args)
        {
            if (args == null) return;
            Targets = args.Targets;
            AimPosition = args.AimPosition;
            AimDirection = args.AimDirection;
            EnergySpent = args.EnergySpent;
        }
    }
}
