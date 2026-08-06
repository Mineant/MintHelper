using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.Inventory
{


    [CreateAssetMenu(menuName = "Mio Helper/Inventory/Inventory Item")]
    public class InventoryItem : BaseItem
    {
        public override BaseGameItem CreateBaseGameInstance()
        {
            return new GameInventoryItem(this);
        }

    }
}
