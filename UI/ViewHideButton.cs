using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mineant
{
    public class ViewHideButton : MonoBehaviour
    {
        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(CloseView);
        }

        private void CloseView()
        {
            GetComponentInParent<View>().Hide();
        }
    }

}
