using System.Collections.Generic;
// using MoreMountains.InventoryEngine;
// using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mineant.Inventory
{
    public class InventoryDragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
    {
        private GraphicRaycaster _raycaster;
        private Canvas _canvas;
        private List<RaycastResult> _raycastResults;
        private BaseGameItemUIProduct _slot;
        private BaseGameItem _item;
        private BaseInventory _inventory;
        private string _playerID;

        private void Awake()
        {
            _raycaster = GetComponent<GraphicRaycaster>();
            _canvas = GetComponent<Canvas>();
        }

        private void Raycast(PointerEventData eventData)
        {
            _raycastResults = new List<RaycastResult>();
            _raycaster.Raycast(eventData, _raycastResults);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _slot = null;
            Raycast(eventData);
            foreach (var result in _raycastResults)
            {
                BaseGameItemUIProduct slot = result.gameObject.GetComponent<BaseGameItemUIProduct>();
                if (slot == null) continue;
                if (slot.BaseGameItem.IsNull()) continue;

                _slot = slot; ;
                _inventory = _slot.InventoryDisplay.Inventory;
                _playerID = _inventory.PlayerID;
                _item = _inventory.GetContent()[_slot.Index];

                break;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_slot == null) return;
            Vector3 screenPoint = Input.mousePosition;
            _slot.IconImage.transform.SetParent(transform, false);
            if (_canvas.worldCamera != null)
            {
                screenPoint.z = _canvas.planeDistance;
                _slot.IconImage.transform.position = _canvas.worldCamera.ScreenToWorldPoint(screenPoint);
            }
            else
                _slot.IconImage.transform.position = screenPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_slot == null) return;
            _slot.IconImage.transform.SetParent(_slot.transform, false);
            _slot.IconImage.transform.localPosition = Vector3.zero;
            Raycast(eventData);

            ///// MINE /////
            // InventoryDragAndDropEvent.Trigger(InventoryDragAndDropEventType.EndDrag, _item);
            ///// END /////


            foreach (var result in _raycastResults)
            {
                BaseGameItemUIProduct destinationSlot = result.gameObject.GetComponent<BaseGameItemUIProduct>();
                if (destinationSlot == null) continue;
                BaseInventory destinationInventory = destinationSlot.InventoryDisplay.Inventory;

                // if (destinationInventory is BaseInventory baseInventory && !baseInventory.IsItemTypeAccepted(_item)) continue;
                if (destinationInventory.GetContent().Length == 0) continue;

                BaseGameItem destinationItem = destinationInventory.GetContent()[destinationSlot.Index];
                bool isDestinationEmpty = destinationItem.IsNull();

                // Move in same inventory
                if (_inventory == destinationInventory)
                {
                    _inventory.MoveItem(_slot.Index, destinationSlot.Index);
                }
                else

                // Move different inventory
                {
                    // Move item to empty slot
                    if (isDestinationEmpty)
                    {
                        BaseGameItem item = _item;
                        destinationInventory.AddItemAt(item, destinationSlot.Index);
                        _inventory.RemoveItem(item);
                    }
                    else

                    // Swap Item
                    {
                        _inventory.RemoveItem(_item);
                        destinationInventory.RemoveItem(destinationItem);

                        _inventory.AddItemAt(destinationItem, _slot.Index);
                        destinationInventory.AddItemAt(_item, destinationSlot.Index);
                    }
                }
                //     if (_inventory == destinationInventory && (_item.CanMoveObject && isDestinationEmpty || _item.CanSwapObject && !isDestinationEmpty && destinationItem.CanSwapObject))
                //         _inventory.MoveItem(_slot.Index, destinationSlot.Index);
                //     else if (_item.IsEquippable && _item.TargetEquipmentInventoryName == destinationInventory.name)
                //     {
                //         if (isDestinationEmpty)
                //         {
                //             _item.Equip(_playerID);
                //             _inventory.MoveItemToInventory(_slot.Index, destinationInventory, destinationSlot.Index);
                //             MMInventoryEvent.Trigger(MMInventoryEventType.ItemEquipped, destinationSlot, destinationInventory.name, _item, 0, destinationSlot.Index, _playerID);
                //         }
                //         else
                //         {
                //             destinationItem.UnEquip(_playerID);
                //             _item.Equip(_playerID);
                //             var tempItem = destinationItem.Copy();
                //             destinationInventory.Content[destinationSlot.Index] = _item.Copy();
                //             _inventory.Content[_slot.Index] = tempItem;
                //             MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventory.name, null, 0, 0, _playerID);
                //             MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, destinationInventory.name, null, 0, 0, _playerID);
                //         }
                //     }
                //     else if (_slot.Unequippable() && _item.TargetInventoryName == destinationInventory.name)
                //     {
                //         if (isDestinationEmpty)
                //         {
                //             _item.UnEquip(_playerID);
                //             _inventory.MoveItemToInventory(_slot.Index, destinationInventory, destinationSlot.Index);
                //         }
                //         else if (destinationItem.IsEquippable && destinationItem.TargetEquipmentInventoryName == _inventory.name)
                //         {
                //             _item.UnEquip(_playerID);
                //             destinationItem.Equip(_playerID);
                //             var tempItem = _item.Copy();
                //             _inventory.Content[_slot.Index] = destinationItem.Copy();
                //             destinationInventory.Content[destinationSlot.Index] = tempItem;
                //             MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, destinationInventory.name, null, 0, 0, _playerID);
                //             MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventory.name, null, 0, 0, _playerID);
                //         }
                //     }
                //     else if (_inventory != destinationInventory && isDestinationEmpty)
                //     {
                //         _inventory.MoveItemToInventory(_slot.Index, destinationInventory, destinationSlot.Index);
                //         MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventory.name, null, 0, 0, _playerID);
                //         MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, destinationInventory.name, null, 0, 0, _playerID);
                //     }
                //     /////// MINE //////
                //     else if (_inventory != destinationInventory && (_item.CanSwapObject && !isDestinationEmpty && destinationItem.CanSwapObject))
                //     {
                //         _inventory.MoveItemToInventory(_slot.Index, destinationInventory, destinationSlot.Index);
                //         MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventory.name, null, 0, 0, _playerID);
                //         MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, destinationInventory.name, null, 0, 0, _playerID);
                //     }
                //     ////// END /////

                //     return;
            }
            // _slot.Drop();

        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            // Disable the drag theshold to trigger drag instantly
            // which let us manage filtering and clipping ourselves
            eventData.useDragThreshold = false;

        }
    }
}

// public enum InventoryDragAndDropEventType { BeginDrag, Drag, EndDrag }

// public struct InventoryDragAndDropEvent
// {
//     public InventoryDragAndDropEventType EventType;
//     public InventoryItem Item;

//     public InventoryDragAndDropEvent(InventoryDragAndDropEventType eventType, InventoryItem item)
//     {
//         EventType = eventType;
//         Item = item;
//     }

//     static InventoryDragAndDropEvent e;

//     public static void Trigger(InventoryDragAndDropEventType eventType, InventoryItem item)
//     {
//         e.EventType = eventType;
//         e.Item = item;

//         MMEventManager.TriggerEvent(e);
//     }
// }