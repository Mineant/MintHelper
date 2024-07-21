using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Mineant.Inventory
{
    public abstract class BaseInventory : MonoBehaviour
    {
        public static List<BaseInventory> RegisteredInventories;

        [Tooltip("Used by inventory display")]
        public string InventoryID;

        [Tooltip("the owner of this inventory")]
        public string PlayerID;

        public Action OnChange;

        public abstract void Initialize(int size);

        public static BaseInventory FindInventory(string inventoryName, string playerID)
        {
            if (inventoryName == null)
            {
                return null;
            }

            if (RegisteredInventories == null) return null;

            foreach (BaseInventory inventory in RegisteredInventories)
            {
                if (inventory.InventoryID == inventoryName && inventory.PlayerID == playerID && inventory.GetContent() != null)
                {
                    return inventory;
                }
            }
            return null;
        }

        public abstract BaseGameItem[] GetContent();
        public abstract bool AddItem(BaseGameItem itemToAdd);
        public abstract bool AddItemAt(BaseGameItem itemToAdd, int index);
        public abstract bool RemoveItem(BaseGameItem itemToRemove);
        public abstract bool MoveItem(int start, int end);
        public abstract bool Contains(BaseGameItem item);
        public abstract int GetQuantity(string itemID);
    }


    public abstract class BaseInventory<TGameItem> : BaseInventory where TGameItem : BaseGameItem
    {
        public bool DebugMessages;
        // [SerializeField]
        [SerializeReference]
        protected TGameItem[] _content;


        #region 狀態
        public virtual int NumberOfFreeSlots => _content.Length - NumberOfFilledSlots;

        /// whether or not the inventory is full (doesn't have any remaining free slots)
        public virtual bool IsFull => NumberOfFreeSlots <= 0;

        /// The number of filled slots 
        public int NumberOfFilledSlots
        {
            get
            {
                int numberOfFilledSlots = 0;
                for (int i = 0; i < _content.Length; i++)
                {
                    if (!_content[i].IsNull())
                    {
                        numberOfFilledSlots++;
                    }
                }
                return numberOfFilledSlots;
            }
        }
        #endregion


        public override BaseGameItem[] GetContent()
        {
            return _content;
        }

        public virtual TGameItem[] GetTContent()
        {
            return _content;
        }

        public override void Initialize(int size)
        {
            // Should set the maximum size, etc....
            _content = new TGameItem[size];

            DebugMessage("Initialized");

            UpdateInventoryUI();

        }





        /// <summary>
        /// Tries to add an item of the specified type. Note that this is name based.
        /// </summary>
        /// <returns><c>true</c>, if item was added, <c>false</c> if it couldn't be added (item null, inventory full).</returns>
        /// <param name="itemToAdd">Item to add.</param>
        public override bool AddItem(BaseGameItem itemToAdd)
        {
            if (itemToAdd.IsNull()) return false;
            if (!itemToAdd.GetBaseParent().CanStack() && IsFull) return false;

            // FInd empty index
            int index = -1;

            // If the item can stack, try to find a slot with the same type
            if (itemToAdd.GetBaseParent().CanStack())
            {
                for (int i = 0; i < _content.Length; i++)
                {
                    // Find the same item
                    if (!_content[i].IsNull() && _content[i].GetBaseParent().ItemID == itemToAdd.GetBaseParent().ItemID)
                    {
                        // If we add theh item and it doesnt exist the maximum amount
                        if (itemToAdd.GetBaseParent().GetMaxStacks() >= _content[i].Quantity + itemToAdd.Quantity)
                        {
                            index = i;
                            break;
                        }
                    }
                }
            }

            // If we couldn't find a slot with the same type or the item is not stackable, try to find an empty slot
            if (index == -1)
            {
                for (int i = 0; i < _content.Length; i++)
                {
                    if (_content[i].IsNull())
                    {
                        index = i;
                        break;
                    }
                }
            }

            // If we couldn't find an empty slot, return false
            if (index == -1)
            {
                DebugMessage($"Cannot find index to add {itemToAdd.Quantity}x [{itemToAdd.GetBaseParent().Name}]");
                return false;
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
        public override bool AddItemAt(BaseGameItem itemToAdd, int destinationIndex)
        {
            if (destinationIndex < 0 || destinationIndex >= _content.Length)
            {
                DebugMessage($"Content length too short to add item.");
                return false;
            }

            // Return if the item is null
            if (itemToAdd.IsNull())
            {
                DebugMessage($"itemToAdd is null");
                return false;
            }

            // Return if item cannot stack but inventory is full
            if (!itemToAdd.GetBaseParent().CanStack() && IsFull)
            {
                DebugMessage($"item cannot stack but inventory is full");
                return false;
            }

            // Return if item cannot stack but destination slot is not empty
            if (!itemToAdd.GetBaseParent().CanStack() && !_content[destinationIndex].IsNull())
            {
                DebugMessage($"item cannot stack but destination slot is not empty");
                return false;
            }

            // Check if adding the same item
            if (itemToAdd == _content[destinationIndex])
            {
                DebugMessage($"item {itemToAdd.GetBaseParent().Name} already exists in slot {destinationIndex}");
                return false;
            }

            // If the destination slot is empty, we can add the item directly
            if (_content[destinationIndex].IsNull())
            {
                _content[destinationIndex] = (TGameItem)itemToAdd;
                // DebugMessage($"Item {itemToAdd.GetBaseParent().Name} x{itemToAdd.Quantity} added to slot {destinationIndex}");
            }

            // We need to check if the item can stack with the existing item in the slot
            else if (itemToAdd.GetBaseParent().CanStack() && // Item Can Stack
                _content[destinationIndex].GetBaseParent().ItemID == itemToAdd.GetBaseParent().ItemID &&  // Adding the same item
                itemToAdd.GetBaseParent().GetMaxStacks() >= _content[destinationIndex].Quantity + itemToAdd.Quantity)   // Adding doesn't exceed maximum stack size
            {
                _content[destinationIndex].Quantity += itemToAdd.Quantity;
                // DebugMessage($"Item {itemToAdd.GetBaseParent().Name} x{itemToAdd.Quantity} added to slot {destinationIndex}");
            }
            else
            {
                // Cannot add item
                DebugMessage("Cannot add item");
                return false;
            }

            DebugMessage($"item {itemToAdd.GetBaseParent().Name} x{itemToAdd.Quantity} added to slot {destinationIndex}");
            UpdateInventoryUI();

            return true;
        }

        /// <summary>
        /// Tries to move the item at the first parameter slot to the second slot
        /// </summary>
        /// <returns><c>true</c>, if item was moved, <c>false</c> otherwise.</returns>
        /// <param name="startIndex">Start index.</param>
        /// <param name="endIndex">End index.</param>
        public override bool MoveItem(int startIndex, int endIndex)
        {
            if (_content[startIndex].IsNull()) return false;

            TGameItem startItem = _content[startIndex];
            TGameItem endItem = _content[endIndex];

            // See if we can add the item to the destination slot, since maybe the item is stackable
            if (AddItemAt(startItem, endIndex))
            {
                _content[startIndex] = null;
                DebugMessage($"Item {startItem.GetBaseParent().Name} moved from slot {startIndex} to slot {endIndex}");
            }
            // Swap Items
            else
            {
                _content[startIndex] = endItem;
                _content[endIndex] = startItem;
                DebugMessage($"Item {startItem.GetBaseParent().Name} swapped with item in slot {endIndex}");
            }

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
        public virtual bool MoveItemToInventory(int startIndex, BaseInventory targetInventory, int endIndex = -1)
        {
            TGameItem itemToMove = _content[startIndex];

            bool moved = false;

            // if we've specified a destination index, we use it, otherwise we add normally
            if (endIndex >= 0)
            {
                moved = targetInventory.AddItemAt(itemToMove, endIndex);
            }
            else
            {
                moved = targetInventory.AddItem(itemToMove);
            }

            // If we couldn't add the item, return false
            if (!moved)
            {
                DebugMessage($"Could not add item to inventory");
                return false;
            }

            DebugMessage($"Item {itemToMove.GetBaseParent().Name} moved to inventory {targetInventory.InventoryID}");

            // we then remove from the original inventory
            RemoveItem(startIndex, itemToMove.Quantity);

            return true;
        }


        /// <summary>
        /// Removes the specified item from the inventory.
        /// </summary>
        /// <returns><c>true</c>, if item was removed, <c>false</c> otherwise.</returns>
        /// <param name="itemToRemove">Item to remove.</param>
        public virtual bool RemoveItem(int index, int quantity)
        {
            // Index out of range
            if (index < 0 || index >= _content.Length)
            {
                DebugMessage($"Index out of range {index}");
                return false;
            }

            // Item is null
            if (_content[index].IsNull())
            {
                DebugMessage($"Item is null");
                return false;
            }

            // Cannot remove more than the content's quantity.
            if (quantity > _content[index].Quantity)
            {
                DebugMessage($"Cannot remove more than the content's quantity. {quantity} > {_content[index].Quantity}");
                return false;
            }

            _content[index].Quantity -= quantity;

            // Check if the item has been completely removed
            if (_content[index].Quantity <= 0)
            {
                _content[index] = null;
            }

            DebugMessage($"Item {index} x{quantity} removed. ");
            UpdateInventoryUI();

            return true;
        }

        /// <summary>
        /// This will find the item, and remove it from the inventory WITHOUT changing the quantity.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public override bool RemoveItem(BaseGameItem item)
        {
            if (item.IsNull()) return false;

            int index = Array.IndexOf(_content, item);
            if (index == -1) return false;

            _content[index] = null;
            UpdateInventoryUI();

            return true;
        }

        /// <summary>
        /// This will remove the specified quantity of the specified item from the inventory. This will remove multiple stacks
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public virtual bool RemoveItemByID(string itemID, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemID)) return false;

            List<int> foundItems = InventoryContains(itemID);
            if (foundItems.Count == 0) return false;



            // if stackable, find the item with just enough quantity to remove
            if (_content[foundItems[0]].GetBaseParent().CanStack())
            {
                // See if we have enough quantity to remove
                if (GetQuantity(itemID) >= quantity)
                {
                    int quantityToRemove = quantity;

                    // Loop through the founded items
                    for (int i = 0; i < foundItems.Count; i++)
                    {
                        int itemQuantity = _content[foundItems[i]].Quantity;

                        // If The item has enough quantity to remove, remove it and break the loop
                        if (itemQuantity >= quantityToRemove)
                        {
                            RemoveItem(foundItems[i], quantityToRemove);
                            // DebugMessage($"Removed {quantityToRemove} of item {itemID} from slot {foundItems[i]}");
                            break;
                        }

                        // If the item has more than enough quantity, remove the required quantity and set the remaining quantity to null
                        else
                        {
                            quantityToRemove -= itemQuantity;
                            _content[foundItems[i]] = null;
                            DebugMessage($"Removed {itemQuantity} of item {itemID} from slot {foundItems[i]}");
                        }
                    }
                }
            }
            else
            {
                RemoveItem(foundItems[0], quantity);
            }

            UpdateInventoryUI();

            return false;
        }

        /// <summary>
        /// Destroys the item stored at index i
        /// </summary>
        /// <returns><c>true</c>, if item was destroyed, <c>false</c> otherwise.</returns>
        /// <param name="i">The index.</param>
        public virtual bool DestroyItem(int i)
        {
            _content[i] = null;
            UpdateInventoryUI();
            return true;
        }

        /// <summary>
        /// Empties the current state of the inventory.
        /// </summary>
        public virtual void EmptyInventory()
        {
            _content = new TGameItem[_content.Length];
        }


        public virtual List<int> InventoryContains(string searchedItemID)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < _content.Length; i++)
            {
                if (!_content[i].IsNull())
                {
                    if (_content[i].GetBaseParent().ItemID == searchedItemID)
                    {
                        list.Add(i);
                    }
                }
            }
            return list;
        }



        public override bool Contains(BaseGameItem item)
        {
            return _content.Contains(item);
        }

        public override int GetQuantity(string itemID)
        {
            List<int> foundItems = InventoryContains(itemID);
            if (foundItems.Count == 0) return -1;

            // We will see if all the items with the same id have the required quantity
            int totalQuantity = 0;
            for (int i = 0; i < foundItems.Count; i++)
            {
                totalQuantity += _content[foundItems[i]].Quantity;
            }

            return totalQuantity;
        }

        public virtual void UpdateInventoryUI()
        {
            if (InventoryDisplay.RegisteredInventoryDisplays == null) return;

            foreach (InventoryDisplay display in InventoryDisplay.RegisteredInventoryDisplays)
            {
                if (display.Inventory == this)
                {
                    display.Generate(this);
                }
            }

            if (OnChange != null) OnChange.Invoke();
        }

        protected virtual void DebugMessage(string msg)
        {
            if (!DebugMessages) return;

            Debug.Log($"Inventory [{InventoryID}] : {msg}");
        }

        protected virtual void OnEnable()
        {
            // Register inventory
            if (RegisteredInventories == null)
            {
                RegisteredInventories = new List<BaseInventory>();
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

        protected virtual void OnDisable()
        {
            RegisteredInventories.Remove(this);
        }

    }
}
