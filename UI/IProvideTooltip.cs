using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public interface IProvideTooltip
    {
        TooltipArgs GetTooltip();
        bool CanProvideTooltip();
    }
}
