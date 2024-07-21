using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant.Inventory.Example
{
    [CreateAssetMenu]
    public class ResourceItem : BaseItem
    {
        [Header("Resource")]
        public ResourceType ResourceType;


        public override bool CanStack()
        {
            if (ResourceType == ResourceType.Single)
                return false;
            else
                return true;
        }

        public override BaseGameItem CreateBaseGameInstance()
        {
            return new ResourceGameItem(this);
        }

        public override int GetMaxStacks()
        {
            switch (ResourceType)
            {
                case ResourceType.Single:
                    return 1;
                case ResourceType.Normal:
                    return 99;
                case ResourceType.Currency:
                    return 9999;
                case ResourceType.Unlimited:
                    return 999999999;
                default:
                    return -1;
            }
        }
    }
}