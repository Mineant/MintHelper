using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Mineant.Inventory.Example
{
    public class BuyThingButton : MonoBehaviour
    {
        public BaseItem item;
        public int quantity;
        public BaseItem costItem;
        public int costItemQuantity;

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(BuyThing);
        }

        private void BuyThing()
        {
            FindObjectOfType<InventoryTestExampleManager>().BuyItem(item, quantity, costItem, costItemQuantity);
        }
    }
}