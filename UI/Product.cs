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
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
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

        protected Action<Product<TArgs>> OnProductInteractorClick;

        protected virtual void Awake()
        {
            if (ProductButton) ProductButton.onClick.AddListener(ProductInteractorClick);
            if (ProductToggle) ProductToggle.onValueChanged.AddListener((value) => { if (value) ProductInteractorClick(); });
        }

        protected virtual void ProductInteractorClick()
        {
            if (OnProductInteractorClick != null) OnProductInteractorClick.Invoke(this);
        }

        public virtual void SetProductInteractorClick(Action<Product<TArgs>> onProductButtonClick)
        {
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



    }


}