using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mineant
{
    public class GeneralUIProduct : Product<GeneralUIProductArgs>
    {
        public TMP_Text Text1;
        public TMP_Text Text2;
        public Image Image1;
        public Image Image2;

        public override void Generate(GeneralUIProductArgs productArgs)
        {
            base.Generate(productArgs);

            if (Text1 != null && productArgs.Text1 != null) Text1.text = productArgs.Text1;
            if (Text2 != null && productArgs.Text2 != null) Text2.text = productArgs.Text2;
            if (Image1 != null && productArgs.Image1 != null) Image1.sprite = productArgs.Image1;
            if (Image2 != null && productArgs.Image2 != null) Image2.sprite = productArgs.Image2;
        }
    }

    public class GeneralUIProductArgs : ProductArgs
    {
        public string Text1;
        public string Text2;
        public Sprite Image1;
        public Sprite Image2;

        public GeneralUIProductArgs(string primaryText = null, string secondaryText = null, Sprite image1 = null, Sprite iamge2 = null)
        {
            Text1 = primaryText;
            Text2 = secondaryText;
            Image1 = image1;
            Image2 = iamge2;
        }

    }
}
