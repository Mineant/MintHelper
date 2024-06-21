using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mineant
{
    public class ExampleMVCView : MVCView<ExampleMVCController, MVCWeapon>
    {
        public TMP_Text TitleText;
        public Image IconImage;
        public Button AddAmmoButton;
        public Button ReduceAmmoButton;

        protected override void Awake()
        {
            base.Awake();

            AddAmmoButton.onClick.AddListener(OnClickAddAmmoButotn);
            ReduceAmmoButton.onClick.AddListener(OnClickReduceAmmoButton);
        }

        public void OnClickAddAmmoButotn() => _controller?.AddAmmoClick();
        public void OnClickReduceAmmoButton() => _controller?.ReduceAmmoClick();
    }

}
