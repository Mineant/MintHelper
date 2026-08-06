using System;
using System.Collections.Generic;
using MioHelper.TextFormat;
using UnityEngine;

namespace MioHelper.Tooltip
{
    /// <summary>
    /// Raw tooltip content authored in a provider. <see cref="Name"/> and
    /// <see cref="Description"/> are MioText template strings (may contain {L:}, {C:}, {N:},
    /// @{key}); the manager formats them through <see cref="MioTextFormatter"/> before display.
    /// </summary>
    [Serializable]
    public class MioTooltipContent
    {
        [Tooltip("Keyword matched by {L:keyword} link ids. Case-insensitive.")]
        public string Keyword;

        [Tooltip("Name template. May contain {C:}, {L:}, @{key}.")]
        public string Name;

        [Tooltip("Description template. May contain {C:}, {L:}, @{key}.")]
        [TextArea(1, 8)]
        public string Description;

        [Tooltip("Optional icon shown in TooltipUIProduct.IconImage.")]
        public Sprite Icon;

        [Tooltip("Values for @{key} placeholders used by Name/Description.")]
        public List<MioTextParameter> Values = new List<MioTextParameter>();
    }
}
