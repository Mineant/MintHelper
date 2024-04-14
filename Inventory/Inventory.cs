using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Mineant.Inventory
{
    public class Inventory : MonoBehaviour
    {
        public static List<Inventory> RegisteredInventories;

        public static Inventory FindInventory(string inventoryName, string playerID)
        {
            if (inventoryName == null)
            {
                return null;
            }

            if (RegisteredInventories == null) return null;

            foreach (Inventory inventory in RegisteredInventories)
            {
                if (inventory.InventoryID == inventoryName && inventory.PlayerID == playerID)
                {
                    return inventory;
                }
            }
            return null;
        }


        [Tooltip("Used by inventory display")]
        public string InventoryID;

        [Tooltip("the owner of this inventory")]
        public string PlayerID;
        public GameInventoryItem[] Content;


        #region 狀態
        public virtual int NumberOfFreeSlots => Content.Length - NumberOfFilledSlots;

        /// whether or not the inventory is full (doesn't have any remaining free slots)
        public virtual bool IsFull => NumberOfFreeSlots <= 0;

        /// The number of filled slots 
        public int NumberOfFilledSlots
        {
            get
            {
                int numberOfFilledSlots = 0;
                for (int i = 0; i < Content.Length; i++)
                {
                    if (!Content[i].IsNull())
                    {
                        numberOfFilledSlots++;
                    }
                }
                return numberOfFilledSlots;
            }
        }
        #endregion


        public void Initialize(int size)
        {
            // Should set the maximum size, etc....
            Content = new GameInventoryItem[size];

            // Register inventory
            if (RegisteredInventories == null)
            {
                RegisteredInventories = new List<Inventory>();
            }
            if (RegisteredInventories.Count > 0)
            {
                for (int i = RegisteredInventories.Count - 1; i >= 0; i--)
                {
                    if (RegisteredInventories[i] == null)
                    {
                        RegisteredInventories.RemoveAt(i);
                    }
                }
            }
            RegisteredInventories.Add(this);
        }





        /// <summary>
        /// Tries to add an item of the specified type. Note that this is name based.
        /// </summary>
        /// <returns><c>true</c>, if item was added, <c>false</c> if it couldn't be added (item null, inventory full).</returns>
        /// <param name="itemToAdd">Item to add.</param>
        public virtual bool AddItem(GameInventoryItem itemToAdd)
        {
            if (itemToAdd.IsNull()) return false;
            if (IsFull) return false;

            // FInd empty index
            int index = -1;
            for (int i = 0; i < Content.Length; i++)
            {
                if (Content[i].IsNull())
                {
                    index = i;
                    break;
                }
            }

            return AddItemAt(itemToAdd, index);
        }

        /// <summary>
        /// Adds the specified quantity of the specified item to the inventory, at the destination index of choice
        /// </summary>
        /// <param name="itemToAdd"></param>
        /// <param name="quantity"></param>
        /// <param name="destinationIndex"></param>
        /// <returns></returns>
        public virtual bool AddItemAt(GameInventoryItem itemToAdd, int destinationIndex)
        {
            if (itemToAdd.IsNull()) return false;
            if (IsFull) return false;
            if (!Content[destinationIndex].IsNull()) return false;

            Content[destinationIndex] = itemToAdd;
            UpdateInventoryUI();

            return true;
        }

        /// <summary>
        /// Tries to move the item at the first parameter slot to the second slot
        /// </summary>
        /// <returns><c>true</c>, if item was moved, <c>false</c> otherwise.</returns>
        /// <param name="startIndex">Start index.</param>
        /// <param name="endIndex">End index.</param>
        public virtual bool MoveItem(int startIndex, int endIndex)
        {
            if (Content[startIndex].IsNull()) return false;

            GameInventoryItem startItem = Content[startIndex];
            GameInventoryItem endItem = Content[endIndex];
            Content[startIndex] = endItem;
            Content[endIndex] = startItem;

            UpdateInventoryUI();

            return true;
        }

        /// <summary>
        /// This method lets you move the item at startIndex to the chosen targetInventory, at an optional endIndex there
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="targetInventory"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public virtual bool MoveItemToInventory(int startIndex, Inventory targetInventory, int endIndex = -1)
        {
            if (Content[startIndex].IsNull()) return false;
            if (endIndex >= 0 && !targetInventory.Content[endIndex].IsNull()) return false;

            GameInventoryItem itemToMove = Content[startIndex];

            // if we've specified a destination index, we use it, otherwise we add normally
            if (endIndex >= 0)
            {
                targetInventory.AddItemAt(itemToMove, endIndex);
            }
            else
            {
                targetInventory.AddItem(itemToMove);
            }

            // we then remove from the original inventory
            RemoveItem(startIndex);

            return true;
        }

        /// <summary>
        /// Removes the specified item from the inventory.
        /// </summary>
        /// <returns><c>true</c>, if item was removed, <c>false</c> otherwise.</returns>
        /// <param name="itemToRemove">Item to remove.</param>
        public virtual bool RemoveItem(int i)
        {
            if (i < 0 || i >= Content.Length) return false;
            if (Content[i].IsNull()) return false;

            Content[i] = null;
            UpdateInventoryUI();

            return true;
        }

        public virtual bool RemoveItem(GameInventoryItem item)
        {
            if (item.IsNull()) return false;

            int index = Array.IndexOf(Content, item);
            return RemoveItem(index);
        }

        /// <summary>
        /// Removes the specified quantity of the item matching the specified itemID
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public virtual bool RemoveItemByID(string itemID)
        {
            if (string.IsNullOrEmpty(itemID)) return false;

            List<int> foundItems = InventoryContains(itemID);
            if (foundItems.Count == 0) return false;

            return RemoveItem(foundItems[0]);
        }

        /// <summary>
        /// Destroys the item stored at index i
        /// </summary>
        /// <returns><c>true</c>, if item was destroyed, <c>false</c> otherwise.</returns>
        /// <param name="i">The index.</param>
        public virtual bool DestroyItem(int i)
        {
            Content[i] = null;
            UpdateInventoryUI();
            return true;
        }

        /// <summary>
        /// Empties the current state of the inventory.
        /// </summary>
        public virtual void EmptyInventory()
        {
            Content = new GameInventoryItem[Content.Length];
        }


        public virtual List<int> InventoryContains(string searchedItemID)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < Content.Length; i++)
            {
                if (!Content[i].IsNull())
                {
                    if (Content[i].Parent.ItemID == searchedItemID)
                    {
                        list.Add(i);
                    }
                }
            }
            return list;
        }

        public void UpdateInventoryUI()
        {
            if (InventoryDisplay.RegisteredInventoryDisplays == null) return;

            foreach (InventoryDisplay display in InventoryDisplay.RegisteredInventoryDisplays)
            {
                if (display.Inventory == this)
                {
                    display.Generate(this);
                }
            }
        }

    }
}
