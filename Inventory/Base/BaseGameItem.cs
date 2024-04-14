using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant.Inventory
{
    public abstract class BaseGameItem
    {
        public abstract BaseItem GetParent();
    }

    public abstract class BaseGameItem<TItem> : BaseGameItem where TItem : BaseItem
    {
        protected TItem _parent;

        public BaseGameItem(TItem parent)
        {
            _parent = parent;
        }

        public override BaseItem GetParent()
        {
            return _parent;
        }

    }
}
