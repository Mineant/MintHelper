using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Mineant.Inventory
{
    public class TestInventoryPlayer : MonoBehaviour
    {
        public int InventorySize;

        public GameInventory Inventory;

        public List<InventoryItem> DefaultItems;

        void Start()
        {
            Inventory.Initialize(InventorySize);
            foreach (InventoryItem item in DefaultItems)
            {
                Inventory.AddItem(new GameInventoryItem(item));
            }
        }
    }
}