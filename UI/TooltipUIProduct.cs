using System.Collections;
using System.Collections.Generic;
using Mineant;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core
{
    public class TooltipUIProduct : Product<TooltipProductArgs>
    {
        public Image IconImage;
        public TMP_Text NameText;
        public TMP_Text DescriptionText;

        public override void Generate(TooltipProductArgs productArgs)
        {
            base.Generate(productArgs);

            TooltipArgs args = productArgs.TooltipArgs;
            if (NameText) NameText.text = args.Name;
            if (DescriptionText) DescriptionText.text = args.Description;
            if (IconImage) IconImage.sprite = args.Icon;
        }
    }

    public class TooltipProductArgs : ProductArgs
    {
        public TooltipArgs TooltipArgs;
        public TooltipProductArgs(TooltipArgs tooltipArgs)
        {
            TooltipArgs = tooltipArgs;
        }

    }
}
