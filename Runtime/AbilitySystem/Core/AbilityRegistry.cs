using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using MioHelper;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Attribute that registers an ability component class with the AbilityRegistry.
    /// Place this on any AbilityEffect, AbilityTrigger, AbilityCondition, or AbilitySelector subclass
    /// to make it discoverable without editing framework code.
    ///
    /// Usage: [RegisterAbilityEffect("AE_ShootProjectile")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RegisterAbilityEffectAttribute : Attribute
    {
        public string Identifier { get; }
        public RegisterAbilityEffectAttribute(string identifier) => Identifier = identifier;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RegisterAbilityTriggerAttribute : Attribute
    {
        public string Identifier { get; }
        public RegisterAbilityTriggerAttribute(string identifier) => Identifier = identifier;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RegisterAbilityConditionAttribute : Attribute
    {
        public string Identifier { get; }
        public RegisterAbilityConditionAttribute(string identifier) => Identifier = identifier;
    }

    /// <summary>
    /// Central registry for ability components. Uses attribute-based registration so that
    /// adding a new effect/trigger/condition requires only writing a class with the appropriate
    /// [RegisterAbility*] attribute — no framework edits.
    ///
    /// Scanning happens on first use (lazy) or can be triggered explicitly via BuildRegistry().
    /// </summary>
    public static class AbilityRegistry
    {
        private static bool _initialized;
        private static Dictionary<string, Type> _effectTypes = new();
        private static Dictionary<string, Type> _triggerTypes = new();
        private static Dictionary<string, Type> _conditionTypes = new();

        /// <summary>
        /// Explicitly build the registry by scanning all loaded assemblies.
        /// Called automatically on first access if not yet initialized.
        /// </summary>
        public static void BuildRegistry()
        {
            if (_initialized) return;

            _effectTypes = new Dictionary<string, Type>();
            _triggerTypes = new Dictionary<string, Type>();
            _conditionTypes = new Dictionary<string, Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Skip system assemblies for performance
                var assemblyName = assembly.GetName().Name;
                if (assemblyName.StartsWith("System") || assemblyName.StartsWith("mscorlib")
                    || assemblyName.StartsWith("UnityEditor") || assemblyName.StartsWith("UnityEngine"))
                    continue;

                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException) { continue; }

                foreach (var type in types)
                {
                    if (type.IsAbstract) continue;

                    // Effects
                    var effectAttr = type.GetCustomAttribute<RegisterAbilityEffectAttribute>();
                    if (effectAttr != null && typeof(AbilityEffect).IsAssignableFrom(type))
                        _effectTypes[effectAttr.Identifier] = type;

                    // Triggers
                    var triggerAttr = type.GetCustomAttribute<RegisterAbilityTriggerAttribute>();
                    if (triggerAttr != null && typeof(AbilityTrigger).IsAssignableFrom(type))
                        _triggerTypes[triggerAttr.Identifier] = type;

                    // Conditions
                    var condAttr = type.GetCustomAttribute<RegisterAbilityConditionAttribute>();
                    if (condAttr != null && typeof(AbilityCondition).IsAssignableFrom(type))
                        _conditionTypes[condAttr.Identifier] = type;
                }
            }

            _initialized = true;
        }

        /// <summary>
        /// Clear the registry. Useful for tests or when assemblies are reloaded.
        /// </summary>
        public static void ClearRegistry()
        {
            _initialized = false;
            _effectTypes.Clear();
            _triggerTypes.Clear();
            _conditionTypes.Clear();
        }

        #region Effect Factory
        public static AbilityEffect CreateEffect(string identifier, bool logError = true)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(identifier)) return null;

            if (_effectTypes.TryGetValue(identifier, out var type))
                return (AbilityEffect)Activator.CreateInstance(type);

            if (logError)
                Debug.LogError($"[AbilityRegistry] No effect registered with identifier: {identifier}");
            return null;
        }
        #endregion

        #region Trigger Factory
        public static AbilityTrigger CreateTrigger(string identifier, bool logError = true)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(identifier)) return null;

            if (_triggerTypes.TryGetValue(identifier, out var type))
                return (AbilityTrigger)Activator.CreateInstance(type);

            if (logError)
                Debug.LogError($"[AbilityRegistry] No trigger registered with identifier: {identifier}");
            return null;
        }
        #endregion

        #region Condition Factory
        public static AbilityCondition CreateCondition(string identifier, bool logError = true)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(identifier)) return null;

            if (_conditionTypes.TryGetValue(identifier, out var type))
                return (AbilityCondition)Activator.CreateInstance(type);

            if (logError)
                Debug.LogError($"[AbilityRegistry] No condition registered with identifier: {identifier}");
            return null;
        }
        #endregion

        private static void EnsureInitialized()
        {
            if (!_initialized) BuildRegistry();
        }
    }
}
