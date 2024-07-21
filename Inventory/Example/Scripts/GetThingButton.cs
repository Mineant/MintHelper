using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Mineant.Inventory.Example
{
    public class GetThingButton : MonoBehaviour
    {
        public BaseItem item;
        public int quantity;
        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(GetThing);
        }

        private void GetThing()
        {
            FindObjectOfType<InventoryTestExampleManager>().GetItem(item, quantity);
        }
    }
}