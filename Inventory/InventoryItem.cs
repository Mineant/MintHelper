using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Mineant.Inventory
{
    [CreateAssetMenu]
    public class InventoryItem : ScriptableObject
    {
        public string ItemID;

        public string Name;

        public Sprite Icon;
    }

    public static class InventoryItemExtensions
    {
        public static bool IsNull(this InventoryItem item)
        {
            if (item == null) return true;

            return false;
        }

        public static bool IsNull(this GameInventoryItem item)
        {
            if (item == null) return true;
            if (item.Parent == null) return true;

            return false;
        }
    }
}
