using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Mineant
{

    public abstract class Product : MonoBehaviour
    {


        public virtual void Show()
        {
        }

        public virtual void Hide()
        {
        }


        /// <summary>
        /// Resetting values of the product. Used useful for Item slots where when it is empty, no icons, names are shown.
        /// </summary>
        public virtual void Reset()
        {

        }
    }

    public abstract class Product<TArgs> : Product where TArgs : ProductArgs
    {
        [Tooltip("Product Button is used for showing more information about the product.")]
        public Button ProductButton;
        public Toggle ProductToggle;
        public bool RebuildAfterGenerate;
        public CanvasGroup CanvasGroup { get; protected set; }
        public LayoutElement LayoutElement { get; protected set; }
        protected Action<Product<TArgs>> OnProductInteractorClick;
        protected ProductActiveMode _activeMode;
        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            LayoutElement = GetComponent<LayoutElement>();
            if (ProductButton) ProductButton.onClick.AddListener(ProductInteractorClick);
            if (ProductToggle) ProductToggle.onValueChanged.AddListener((value) => { if (value) ProductInteractorClick(); });
        }

        protected virtual void ProductInteractorClick()
        {
            if (OnProductInteractorClick != null) OnProductInteractorClick.Invoke(this);
        }

        public virtual void Initialize(ProductActiveMode activeMode)
        {
            _activeMode = activeMode;
        }

        /// <summary>
        /// Use a method to wrap a click, because restrict a product to only trigger one event when click.
        /// </summary>
        public virtual void OnInteract(Action<Product<TArgs>> onProductButtonClick)
        {
            if (ProductButton == null && ProductToggle == null) Debug.LogError("Product interactor cannot be null. Cannt trigger any events.");
            OnProductInteractorClick = onProductButtonClick;
        }

        /// <summary>
        /// Create a product, filling the info, and showing automatically
        /// </summary>
        public virtual void Generate(TArgs productArgs)
        {
            // Call base.Initalize at the end
            Show();
            OnProductInteractorClick = null;

            if (RebuildAfterGenerate) LayoutRebuilder.MarkLayoutForRebuild(this.GetComponent<RectTransform>());

            if (TryGetComponent<LayoutUpdater>(out var up))
            {
                up.UpdateLayout();
            }
        }

        public override void Show()
        {
            base.Show();
            ProductSetActive(true);
        }

        public override void Hide()
        {
            base.Hide();
            ProductSetActive(false);
        }
        
        protected virtual void ProductSetActive(bool value)
        {
            switch (_activeMode)
            {
                case ProductActiveMode.GameObject:
                    gameObject.SetActive(value);
                    break;
                case ProductActiveMode.CanvasGroupLayoutElement:
                    CanvasGroup.alpha = value ? 1f : 0f;
                    LayoutElement.ignoreLayout = !value;
                    break;
            }
        }

    }


}