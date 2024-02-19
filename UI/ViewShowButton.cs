using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mineant
{
    public class ViewShowButton : MonoBehaviour
    {
        public View TargetView;
        public ViewMode ViewMode;

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(ShowView);
        }

        private void ShowView()
        {
            ViewManager.Instance.Show(TargetView, null, ViewMode);
        }
    }

}