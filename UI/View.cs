using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mineant
{

    public abstract class View : MonoBehaviour
    {
        public bool HideWithCanvas;
        public MMF_Player OnShowFeedbacks;
        public MMF_Player OnHideFeedbacks;

        public bool IsInitialized { get; private set; }
        public Action OnHide;
        public Action OnShow;

        protected Canvas _canvas;
        // protected GraphicRaycaster _graphicRaycaster;
        protected CanvasGroup _canvasGroup;

        void Awake()
        {
            GetUIComponents();
        }

        public virtual void Initialize()
        {
            if (IsInitialized) return;

            IsInitialized = true;

            GetUIComponents();
        }

        private void GetUIComponents()
        {
            if (_canvas == null) _canvas = GetComponent<Canvas>();
            // if (_graphicRaycaster == null) _graphicRaycaster = GetComponent<GraphicRaycaster>();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Used for showing the view through Unity Events, since unity events cannot show functions with object[] arguments.
        /// </summary>
        public void ShowNoArgs()
        {
            Show();
        }

        public virtual void Show(object[] args = null)
        {
            GetUIComponents();

            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = true;

            // if (_graphicRaycaster != null)
                // _graphicRaycaster.enabled = true;

            gameObject.SetActive(true);
            if (_canvas) _canvas.enabled = true;

            if (OnShow != null) OnShow.Invoke();
            if (OnShowFeedbacks != null) OnShowFeedbacks.PlayFeedbacks();
        }

        public virtual void Hide()
        {
            // Set the UI
            GetUIComponents();

            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = false;

            // if (_graphicRaycaster != null)
                // _graphicRaycaster.enabled = false;

            if (HideWithCanvas)
            {
                // gameObject.SetActive(true);
                _canvas.enabled = false;
                // if (_graphicRaycaster != null) _graphicRaycaster.enabled = false;

                Debug.Log("Deselecting UI");
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                gameObject.SetActive(false);
            }


            if (OnHide != null) OnHide.Invoke();
            if (OnHideFeedbacks != null) OnHideFeedbacks.PlayFeedbacks();
        }

        private bool HaveCanvas()
        {
            if (_canvas == null) _canvas = GetComponent<Canvas>();
            return _canvas != null;

        }

        public bool IsShowing()
        {
            return gameObject.activeSelf;
        }
    }

}