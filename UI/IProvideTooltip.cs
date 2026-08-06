using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper
{
    public interface IProvideTooltip
    {
        TooltipArgs GetTooltip();
        bool CanProvideTooltip();
    }
}
