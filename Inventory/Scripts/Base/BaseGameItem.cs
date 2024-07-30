using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant.Inventory
{
    [System.Serializable]
    public abstract class BaseGameItem
    {
        public int Quantity;

        [SerializeField]
        protected BaseItem _baseParent;

        protected BaseGameItem()
        {
            Quantity = 1;
            _baseParent = null;
        }

        public abstract BaseItem GetBaseParent();
        public virtual string GetDescription()
        {
            return "BaseGameItem Get Description No Implemented XXXXXXXXXXXX";
        }

    }

    [System.Serializable]
    public abstract class BaseGameItem<TItem> : BaseGameItem where TItem : BaseItem
    {
        [SerializeField]
        protected TItem _parent;

        public BaseGameItem(TItem parent)
        {
            _parent = parent;
            _baseParent = parent;
        }

        public override BaseItem GetBaseParent()
        {
            return _parent;
        }

        public TItem GetParent()
        {
            return _parent;
        }

    }
}
