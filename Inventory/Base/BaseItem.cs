using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant.Inventory
{
    public abstract class BaseItem : ScriptableObject
    {
        public string ItemID;

        public string Name;

        public Sprite Icon;
    }


    public static class InventoryItemExtensions
    {
        public static bool IsNull(this BaseItem item)
        {
            if (item == null) return true;

            return false;
        }

        public static bool IsNull(this BaseGameItem item)
        {
            if (item == null) return true;
            if (item.GetParent() == null) return true;

            return false;
        }
    }
}

