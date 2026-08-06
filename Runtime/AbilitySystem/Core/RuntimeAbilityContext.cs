using System;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.AbilitySystem
{
    /// <summary>
    /// Per-execution blackboard shared across all effects of one ability activation.
    /// Effects can write values here in PreExecute and read them in Execute, enabling
    /// inter-effect communication without direct references.
    ///
    /// Values are stored keyed by string. Scoped access (by component ID) is also supported.
    /// </summary>
    public class RuntimeAbilityContext
    {
        public Dictionary<string, object> Data = new();

        /// <summary>
        /// Try to get a value. Checks component-scoped key first, then global key.
        /// </summary>
        public bool TryGetValue<T>(RuntimeAbilityComponent component, string valueName, out T value)
        {
            if (component != null && Data.TryGetValue(component.ID + valueName, out var scopedValue))
            {
                value = (T)scopedValue;
                return true;
            }
            if (Data.TryGetValue(valueName, out var globalValue))
            {
                value = (T)globalValue;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>Try to get a value, returning a default if not found.</summary>
        public T TryGetValue<T>(RuntimeAbilityComponent component, string valueName, T defaultValue)
        {
            return TryGetValue<T>(component, valueName, out T value) ? value : defaultValue;
        }

        /// <summary>
        /// Set a value in the context. If identity is provided, the key is scoped to that component.
        /// </summary>
        public void SetValue(string valueName, object value, RuntimeAbilityContextIdentity identity = null)
        {
            if (identity != null)
            {
                if (string.IsNullOrEmpty(identity.RuntimeComponentID))
                {
                    Debug.LogError("[AbilitySystem] RuntimeAbilityContextIdentity has empty component ID.");
                    return;
                }
                Data[identity.RuntimeComponentID + valueName] = value;
            }
            else
            {
                Data[valueName] = value;
            }
        }

        public void ClearValues() => Data.Clear();
    }

    /// <summary>
    /// Scopes a context value to a specific runtime component by ID.
    /// </summary>
    public class RuntimeAbilityContextIdentity
    {
        public string RuntimeComponentID;

        public RuntimeAbilityContextIdentity(string componentID)
        {
            RuntimeComponentID = componentID;
        }
    }
}
