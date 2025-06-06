using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Mineant
{
    public abstract class UiWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UiWidgetData BaseData { get; protected set; }
        public abstract void Init(UiWidgetData data);

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            UiWidgetManager.Instance.HideUiWidget(BaseData.Id);
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            // To be implemented
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            // To be implemented
        }

        public virtual void Initialize()
        {

        }

        public virtual void OnDestroy()
        {

        }

    }

    public abstract class PopupMenuUi<T> : UiWidget where T : UiWidgetData
    {
        public T Data { get; protected set; }
        protected bool _isPointerInside;

        public sealed override void Init(UiWidgetData data)
        {
            BaseData = data;
            Data = (T)data;
            Data.OwnerUi = this;

            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            Show();
        }


        protected virtual void Update()
        {
            UpdatePointer();
        }

        protected virtual void UpdatePointer()
        {
            if (Data == null) return;
            if (Data.CloseMode == UiWidgetCloseMode.CloseWhenMouseClickOutside)
            {
                if (Input.GetMouseButtonDown(0) && !_isPointerInside && ShouldAutoHide() && !IsMouseInsideSubPopupMenu())
                {
                    Hide();
                    return;
                }
            }
        }



        protected virtual bool ShouldAutoHide()
        {
            return Data.ShouldAutoHide();
        }

        protected virtual bool IsMouseInsideSubPopupMenu()
        {
            return Data.IsMouseInsideSubUiWidget();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerInside = true;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            _isPointerInside = false;
        }

        protected virtual void OnClickedUI(GameObject ui)
        {
            // Check if the clicked Ui is inside this ui
            RectTransform clickedUi = ui.GetComponent<RectTransform>();
            RectTransform rectTransform = this.GetComponent<RectTransform>();

            if (clickedUi != null && rectTransform != null && clickedUi.IsChildOf(rectTransform)) return;


            if (Data == null) return;
            if (Data.CloseMode == UiWidgetCloseMode.CloseWhenClickOtherUiElements)
            {
                Hide();
                return;
            }
        }

        protected virtual void OnEnable()
        {
            UiWidgetManager.Instance.OnClickedUI += OnClickedUI;
        }



        protected virtual void OnDisable()
        {
            UiWidgetManager.Instance.OnClickedUI -= OnClickedUI;

        }
    }
}