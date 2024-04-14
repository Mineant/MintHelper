using System.Collections;
using System.Collections.Generic;
using Mineant;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mineant.Inventory
{
    public class BaseGameItemUIProduct : Product<BaseGameItemUIProductArgs>
    {
        public TMP_Text NameText;
        public Image IconImage;

        [Header("Data")]
        public int Index;

        public BaseGameItem BaseGameItem { get; protected set; }
        public InventoryDisplay InventoryDisplay { get; protected set; }
        public override void Generate(BaseGameItemUIProductArgs productArgs)
        {
            base.Generate(productArgs);

            BaseGameItem = productArgs.BaseGameItem;
            InventoryDisplay = productArgs.InventoryDisplay;
            Index = productArgs.Index;

            if (IconImage)
            {
                IconImage.gameObject.SetActive(!BaseGameItem.IsNull());
                if (!BaseGameItem.IsNull()) IconImage.sprite = BaseGameItem.GetParent().Icon;
            }
            if (NameText)
            {
                if (!BaseGameItem.IsNull()) NameText.text = BaseGameItem.GetParent().Name;
                else NameText.text = "";
            }
        }
    }

    public class BaseGameItemUIProductArgs : ProductArgs
    {
        public InventoryDisplay InventoryDisplay;
        public BaseGameItem BaseGameItem;
        public int Index;
        public BaseGameItemUIProductArgs(BaseGameItem gameItem, int index, InventoryDisplay inventoryDisplay)
        {
            InventoryDisplay = inventoryDisplay;
            BaseGameItem = gameItem;
            Index = index;
        }
    }
}