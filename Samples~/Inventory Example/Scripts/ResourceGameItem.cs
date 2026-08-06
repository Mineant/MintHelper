using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.Inventory.Example
{
    [System.Serializable]
    public class ResourceGameItem : BaseGameItem<ResourceItem>
    {
        public ResourceGameItem(ResourceItem parent) : base(parent)
        {
        }
    }
}