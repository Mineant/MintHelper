using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Mineant
{

    public class LayoutUpdater : MonoBehaviour
    {
        public TMP_Text TMPTextField;
        public bool AutoUpdateOnEnable = true;
        public int RemainingUpdates;
        public bool AlwaysUpdate;
        const int UPDATE_DURATION = 5;
        public bool IncludeChildrenLayoutGroups;
        public bool IncludeChildrenContentSizeFitter;
        private string _textLastFrame;

        // private LayoutGroup[] _childGroups;

        void Awake()
        {

        }

        void Update()
        {
            // Check text change
            if (TMPTextField != null)
            {
                if (_textLastFrame != TMPTextField.text)
                {
                    _textLastFrame = TMPTextField.text;
                    RemainingUpdates = UPDATE_DURATION;
                    UpdateLayout();
                }
            }

            if (AlwaysUpdate) UpdateLayout();

            if (RemainingUpdates > 0)
            {
                RemainingUpdates--;
                LayoutRebuilder.MarkLayoutForRebuild(GetComponent<RectTransform>());

                if (IncludeChildrenLayoutGroups)
                {
                    foreach (var item in GetComponentsInChildren<LayoutGroup>())
                    {
                        LayoutRebuilder.MarkLayoutForRebuild(item.GetComponent<RectTransform>());
                    }


                    // _childGroups = GetComponentsInChildren<LayoutGroup>();
                }

                if (IncludeChildrenContentSizeFitter)
                {
                    foreach (var item in GetComponentsInChildren<ContentSizeFitter>())
                    {
                        LayoutRebuilder.MarkLayoutForRebuild(item.GetComponent<RectTransform>());
                    }
                }
            }
        }

        void LateUpdate()
        {

        }

        void OnEnable()
        {
            if (AutoUpdateOnEnable)
            {
                UpdateLayout();
            }
        }

        public void UpdateLayout()
        {
            RemainingUpdates = UPDATE_DURATION;
        }
    }

}