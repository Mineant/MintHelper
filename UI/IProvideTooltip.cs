using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant
{
    public interface IProvideTooltip
    {
        TooltipArgs GetTooltip();
        bool CanProvideTooltip();
    }
}
