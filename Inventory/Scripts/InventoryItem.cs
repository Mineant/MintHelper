using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant.Inventory
{


    [CreateAssetMenu(menuName = "Mint Helper/Inventory/Inventory Item")]
    public class InventoryItem : BaseItem
    {
        public override BaseGameItem CreateBaseGameInstance()
        {
            return new GameInventoryItem(this);
        }

    }
}
