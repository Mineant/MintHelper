using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant.Inventory
{
    public abstract class BaseItem : ScriptableObject
    {
        /// <summary>
        /// Identifier of the item.
        /// </summary>
        public string ItemID;

        public string Name;
        public string Description;
        public Sprite Icon;

        public abstract BaseGameItem CreateBaseGameInstance();

        /// <summary>
        /// By default, items cannot stack. Override this method to enable stacking.
        /// in stacking, we assume every item is not unique
        /// </summary>
        /// <returns></returns>
        public virtual bool CanStack()
        {
            return false;
        }

        /// <summary>
        /// By default, items cannot stack. Override this method to set the maximum number of stacks.
        /// </summary>
        /// <returns></returns>
        public virtual int GetMaxStacks()
        {
            return 1;
        }

        public virtual string GetLongDescription()
        {
            return "Long Description Not Implemented XXXXXXXXXXXXX";
        }
    }

    // public class BaseItem<TGameItem> : BaseItem where TGameItem : BaseGameItem
    // {
    //     public override BaseGameItem CreateBaseGameInstance()
    //     {
    //         throw new System.NotImplementedException();
    //     }
    // }


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
            if (item.GetBaseParent() == null) return true;

            return false;
        }
    }
}

