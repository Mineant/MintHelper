using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Mineant.Inventory.Example
{
    public class InventoryTestExampleManager : MonoBehaviour
    {
        [Header("Setting")]
        public List<BaseInventory> Inventories;
        public GameInventory PlayerBag;

        [Header("Operation")]
        public BaseInventory TargetInventory;
        public BaseItem TargetItem;
        public int TargetQuantity;
        public int TargetIndex;

        void Awake()
        {
            Inventories.ForEach(x => x.Initialize(10));
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                var gameItem = TargetItem.CreateBaseGameInstance();
                gameItem.Quantity = TargetQuantity;
                TargetInventory.AddItemAt(gameItem, TargetIndex);
            }
        }

        public void BuyItem(BaseItem item, int quantity, BaseItem costItem, int costItemQuantity)
        {
            // Check if player has enough money
            int playerMoney = PlayerBag.GetQuantity(costItem.ItemID);

            if (playerMoney < costItemQuantity)
            {
                Debug.Log($"Player doesnt have enough money to buy {item.Name}");
                return;
            }

            // Create the game item
            var gameItem = item.CreateBaseGameInstance();
            gameItem.Quantity = quantity;
            bool addedItem = PlayerBag.AddItem(gameItem);
            if (!addedItem)
            {
                Debug.Log("Cannot buy item. Inventory is full");
                return;
            }

            // Remove the cost item from the player bag
            PlayerBag.RemoveItemByID(costItem.ItemID, costItemQuantity);
        }

        public void GetItem(BaseItem item, int quantity)
        {
            var gameItem = item.CreateBaseGameInstance();
            gameItem.Quantity = quantity;
            bool addedItem = PlayerBag.AddItem(gameItem);
        }
    }
}