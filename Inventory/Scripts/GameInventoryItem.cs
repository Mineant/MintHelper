using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant.Inventory
{


    [System.Serializable]
    public class GameInventoryItem : BaseGameItem<InventoryItem>
    {
        public GameInventoryItem(InventoryItem parent) : base(parent)
        {
        }
    }
}
