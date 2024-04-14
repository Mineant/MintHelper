using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Mineant.Inventory
{
    public class InventoryDisplay : MonoBehaviour
    {
        public static List<InventoryDisplay> RegisteredInventoryDisplays;
        public GameInventoryItemUIContainer Container;

        [Header("Listener")]
        public bool Listen;
        public string InventoryID;
        public string PlayerID;

        public Inventory Inventory;

        IEnumerator Start()
        {
            Inventory = null;

            if (Listen)
            {
                while (true)
                {
                    Inventory = Inventory.FindInventory(InventoryID, PlayerID);
                    if (Inventory != null)
                    {
                        Generate(Inventory);
                        break;
                    }
                    yield return null;
                }
            }
        }


        public void Generate(Inventory inventory)
        {
            Container.DestroyAllProducts();

            for (int i = 0; i < inventory.Content.Length; i++)
            {
                GameInventoryItemUIProduct product = Container.GenerateNewProduct(new GameInventoryItemUIProductArgs(inventory.Content[i], i, this));
            }

        }

        void OnEnable()
        {
            if (RegisteredInventoryDisplays == null) RegisteredInventoryDisplays = new();
            if (Listen) RegisteredInventoryDisplays.Add(this);
        }

        void OnDisable()
        {
            if (RegisteredInventoryDisplays == null) RegisteredInventoryDisplays = new();
            if (Listen) RegisteredInventoryDisplays.Remove(this);
        }
    }
}