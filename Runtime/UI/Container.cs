using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MioHelper
{
    public abstract class Container<TProduct, TArgs> : MonoBehaviour where TProduct : Product<TArgs> where TArgs : ProductArgs
    {
        [Tooltip("Products are created under this transform.")]
        public Transform ProductLocation;

        [Tooltip("The product prefab for the pool")]
        public TProduct ProductPrefab;

        public int DefaultSize = 0;

        [Tooltip("Destroys the gameObjects under the targeted transform on start.")]
        public bool DestroyOnStart = true;

        [Tooltip("Add products already placed under the container at start to the pool.")]
        public bool AddExistingProducts;
        public bool AutoUpdateLayout;

        [Tooltip("Uses canvas group and layout element to show products, instead of activate/deactivate. Useful when activating/deactivating GameObjects is expensive.")]
        public ProductActiveMode ActiveMode = ProductActiveMode.GameObject;

        [Header("Advanced")]
        [Tooltip("If a ToggleGroup is found on this object, automatically assigns it to products with toggles.")]
        public bool AutoSetProductToggleGroup;

        [Tooltip("Sets all products to this scale if > 0.")]
        public float CustomScale = -1f;

        [Tooltip("If other scripts reorder products inside this container (e.g. drag-and-drop), enabling this re-sorts the pool list by sibling index so GetActiveProducts returns products in the correct order.")]
        public bool AutoReorderProducts;

        /// <summary>
        /// Fires after products are added/removed. Carries a read-only snapshot of currently active products.
        /// Subscribers never need to call GetActiveProducts themselves.
        /// </summary>
        public event Action<IReadOnlyList<TProduct>> ContentChanged;

        protected List<TProduct> _createProducts;
        private List<TProduct> _activeProductsSnapshot = new();
        private int _batchDepth = 0;
        private int _layoutUpdateFramesRemaining = 0;
        protected const int LAYOUT_UPDATE_FRAMES = 5;

        /// <summary>
        /// Returns a read-only snapshot of currently active products. Zero-allocation — backed by an internal cached list.
        /// </summary>
        public IReadOnlyList<TProduct> ActiveProducts => _activeProductsSnapshot;

        public CanvasGroup CanvasGroup { get; protected set; }
        public LayoutGroup LayoutGroup { get; protected set; }
        public ToggleGroup ToggleGroup { get; protected set; }

        protected bool _initialized = false;

        protected virtual void Awake()
        {
            Init();
        }

        public virtual void Init()
        {
            if (_initialized) return;

            _initialized = true;

            LayoutGroup = ProductLocation.GetComponent<LayoutGroup>();
            ToggleGroup = GetComponent<ToggleGroup>();
            CanvasGroup = GetComponent<CanvasGroup>();
            if (AutoSetProductToggleGroup && ToggleGroup == null) Debug.LogError("Cannot auto set toggle group if no ToggleGroup on container.");
            if (ProductLocation == null) ProductLocation = this.transform;
            if (DestroyOnStart && AddExistingProducts) Debug.LogError("Cannot destroy existing products while trying to add existing products.");
            if (DestroyOnStart)
            {
                foreach (Transform child in ProductLocation)
                {
                    Destroy(child.gameObject);
                }
            }

            _createProducts = new List<TProduct>();

            if (AddExistingProducts)
            {
                foreach (Transform child in ProductLocation.transform)
                {
                    _createProducts.Add(child.GetComponent<TProduct>());
                }
            }

            CreatePool();
        }

        public virtual void ChangeProductPrefab(TProduct productPrefab)
        {
            ProductPrefab = productPrefab;

            if (_createProducts != null)
            {
                foreach (TProduct product in _createProducts)
                {
                    product.Hide();
                    Destroy(product.gameObject);
                }
            }

            _createProducts = new();

            CreatePool();
        }

        /// <summary>
        /// Begin a batch operation. Events and layout updates are suppressed until EndBatch is called.
        /// Supports nesting — only the outermost EndBatch fires events.
        /// </summary>
        public virtual void BeginBatch()
        {
            _batchDepth++;
        }

        /// <summary>
        /// End a batch operation. Fires ContentChanged and schedules layout rebuild once for the entire batch.
        /// </summary>
        public virtual void EndBatch()
        {
            if (_batchDepth <= 0)
            {
                Debug.LogWarning("EndBatch called without matching BeginBatch.");
                return;
            }

            _batchDepth--;
            if (_batchDepth == 0)
            {
                _FireContentChanged();
            }
        }

        protected virtual void LateUpdate()
        {
            if (_layoutUpdateFramesRemaining > 0)
            {
                _layoutUpdateFramesRemaining--;
                if (AutoUpdateLayout) UpdateLayoutGroup();
            }
        }

        protected virtual void CreatePool()
        {
            for (int i = 0; i < DefaultSize; i++)
            {
                CreateNewProduct();
            }
        }

        protected virtual TProduct CreateNewProduct()
        {
            TProduct createdProduct = Instantiate(ProductPrefab, ProductLocation);
            createdProduct.Initialize(ActiveMode);
            _createProducts.Add(createdProduct);
            createdProduct.Hide();
            createdProduct.gameObject.name = $"{createdProduct.gameObject.name}_{_createProducts.Count}";

            if (AutoSetProductToggleGroup && ToggleGroup != null && createdProduct.ProductToggle != null)
            {
                createdProduct.ProductToggle.group = ToggleGroup;
            }

            if (CustomScale > 0f)
            {
                createdProduct.transform.localScale = Vector3.one * CustomScale;
            }

            return createdProduct;
        }

        /// <summary>
        /// Returns the next unused product. Auto-expands the pool if all are active.
        /// </summary>
        protected virtual TProduct GetNextProduct()
        {
            if (AutoReorderProducts) ReorderCreatedProducts();
            for (int i = 0; i < _createProducts.Count; i++)
            {
                if (!IsProductActive(_createProducts[i]))
                {
                    return _createProducts[i];
                }
            }

            // Pool exhausted — expand it
            return CreateNewProduct();
        }

        protected virtual bool IsProductActive(TProduct product)
        {
            switch (ActiveMode)
            {
                case ProductActiveMode.GameObject:
                    return product.gameObject.activeSelf;
                case ProductActiveMode.CanvasGroupLayoutElement:
                    return product.CanvasGroup.alpha > 0f;
            }

            Debug.LogError("Unknown ProductActiveMode");
            return false;
        }

        /// <summary>
        /// Generates a new product from the pool, configures it with the given args, and shows it.
        /// </summary>
        public virtual TProduct GenerateNewProduct(TArgs args, Action<TProduct> onInteract = null)
        {
            Init();

            TProduct product = GetNextProduct();
            product.Generate(args);

            if (_batchDepth == 0)
            {
                _FireContentChanged();
            }

            if (onInteract != null)
            {
                product.OnInteract((x) => { onInteract.Invoke((TProduct)x); });
            }

            return product;
        }

        /// <summary>
        /// Hides all previous products and generates new ones from the args list.
        /// The entire operation fires ContentChanged once.
        /// </summary>
        public virtual List<TProduct> DestroyAndGenerateNewProducts(IEnumerable<TArgs> argsList, Action<TProduct> onInteract = null)
        {
            BeginBatch();

            List<TProduct> products = new();
            DestroyAllProducts();

            foreach (TArgs args in argsList)
            {
                products.Add(GenerateNewProduct(args, onInteract));
            }

            EndBatch();
            return products;
        }

        /// <summary>
        /// Hides all products (returns them to the pool without destroying).
        /// </summary>
        public virtual void DestroyAllProducts()
        {
            Init();

            foreach (TProduct product in _createProducts)
            {
                product.Hide();
            }

            if (_batchDepth == 0)
            {
                _FireContentChanged();
            }
        }

        /// <summary>
        /// Sometimes, if the elements inside the container are reordered by other UI elements,
        /// the container will track products incorrectly. Reordering fixes this.
        /// </summary>
        protected virtual void ReorderCreatedProducts()
        {
            Init();
            _createProducts = _createProducts.OrderBy(p => p.transform.GetSiblingIndex()).ToList();
        }

        /// <summary>
        /// If the container has a LayoutGroup, forces an immediate layout rebuild.
        /// Called automatically by the deferred layout update system.
        /// </summary>
        protected virtual void UpdateLayoutGroup()
        {
            if (LayoutGroup == null) return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(LayoutGroup.transform as RectTransform);
        }

        private void _FireContentChanged()
        {
            // Rebuild the active snapshot
            if (AutoReorderProducts) ReorderCreatedProducts();
            _activeProductsSnapshot.Clear();
            for (int i = 0; i < _createProducts.Count; i++)
            {
                if (IsProductActive(_createProducts[i]))
                {
                    _activeProductsSnapshot.Add(_createProducts[i]);
                }
            }

            _layoutUpdateFramesRemaining = LAYOUT_UPDATE_FRAMES;
            ContentChanged?.Invoke(_activeProductsSnapshot);
        }
    }
}
