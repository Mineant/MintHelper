using System.Collections;
using System.Collections.Generic;
using Mineant;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mineant.Inventory
{
    public class GameInventoryItemUIProduct : Product<GameInventoryItemUIProductArgs>
    {
        public TMP_Text NameText;
        public Image IconImage;

        [Header("Data")]
        public int Index;

        public GameInventoryItem GameInventoryItem { get; protected set; }
        public InventoryDisplay InventoryDisplay { get; protected set; }
        public override void Generate(GameInventoryItemUIProductArgs productArgs)
        {
            base.Generate(productArgs);

            GameInventoryItem = productArgs.GameInventoryItem;
            InventoryDisplay = productArgs.InventoryDisplay;
            Index = productArgs.Index;

            if (IconImage)
            {
                IconImage.gameObject.SetActive(!GameInventoryItem.IsNull());
                if (!GameInventoryItem.IsNull()) IconImage.sprite = GameInventoryItem.Parent.Icon;
            }
            if (NameText)
            {
                if (!GameInventoryItem.IsNull()) NameText.text = GameInventoryItem.Parent.Name;
                else NameText.text = "";
            }
        }
    }

    public class GameInventoryItemUIProductArgs : ProductArgs
    {
        public InventoryDisplay InventoryDisplay;
        public GameInventoryItem GameInventoryItem;
        public int Index;
        public GameInventoryItemUIProductArgs(GameInventoryItem gameInventoryItem, int index, InventoryDisplay inventoryDisplay)
        {
            InventoryDisplay = inventoryDisplay;
            GameInventoryItem = gameInventoryItem;
            Index = index;
        }
    }
}