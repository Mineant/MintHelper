using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Mineant.Inventory
{
    public class InventoryDisplay : MonoBehaviour
    {
        public static List<InventoryDisplay> RegisteredInventoryDisplays;
        public BaseGameInventoryItemUIContainer Container;

        [Header("Listener")]
        public bool Listen;
        public string InventoryID;
        public string PlayerID;

        public BaseInventory Inventory;

        IEnumerator Start()
        {
            Inventory = null;

            if (Listen)
            {
                while (true)
                {
                    Inventory = BaseInventory.FindInventory(InventoryID, PlayerID);
                    if (Inventory != null)
                    {
                        Generate(Inventory);
                        break;
                    }
                    yield return null;
                }
            }
        }


        public void Generate(BaseInventory inventory)
        {
            Container.DestroyAllProducts();

            for (int i = 0; i < inventory.GetContent().Length; i++)
            {
                BaseGameItemUIProduct product = Container.GenerateNewProduct(new BaseGameItemUIProductArgs(inventory.GetContent()[i], i, this));
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