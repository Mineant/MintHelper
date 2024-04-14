using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant.Inventory
{
    [System.Serializable]
    public class GameInventoryItem
    {
        public InventoryItem Parent;

        public GameInventoryItem(InventoryItem item)
        {
            Parent = item;
        }
    }
}
