using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mineant
{
    [RequireComponent(typeof(Button))]
    public class CloseUiWidgetButton : MonoBehaviour
    {
        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(ClosePopupMenu);
        }

        private void ClosePopupMenu()
        {
            GetComponentInParent<UiWidget>().Hide();
        }
    }
}