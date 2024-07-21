using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant.Inventory.Example
{
    [System.Serializable]
    public class ResourceGameItem : BaseGameItem<ResourceItem>
    {
        public ResourceGameItem(ResourceItem parent) : base(parent)
        {
        }
    }
}