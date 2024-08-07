using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Mineant
{
    public abstract class Container : MonoBehaviour
    {
        public Action OnContentChanged; // This will only be called if the products are modified by the container. If the product is directly show/hide by the user, then this will not trigger.F
        private const int HI = 1;   // added this here to let VSCode show references of vairables. fk vscode

    }

    public abstract class Container<TProduct, TArgs> : Container where TProduct : Product<TArgs> where TArgs : ProductArgs
    {
        [Tooltip("Products are created under this transofrm.")]
        public Transform ProductLocation;

        [Tooltip("The product the pool")]
        public TProduct ProductPrefab;

        public int DefaultSize = 0;

        [Tooltip("Destroys the gameObjects under the targetted transform.")]
        public bool DestroyOnStart = true;

        [Tooltip("Add products under the container at start to created products.")]
        public bool AddExistingProducts;
        public bool AutoUpdateLayout;

        [Tooltip("Uses canvas group and layout element to show products, instead of activate / deactivate the products. This is useful activing and deactivating the gameobject consumes a lot of power.")]
        public ProductActiveMode ActiveMode = ProductActiveMode.GameObject;

        [Header("Advanced")]
        [Tooltip("if a toggle group can be found on this object, then if products hv toggle group, will automatically assign.")]
        public bool AutoSetProductToggleGroup;

        [Tooltip("Will set all product to this scale if > 0f.")]
        public float CustomScale = -1f;

        [Tooltip("If other scripts will reorder the products inside this container, for example, moave the products to another parent, or change the sibling index, enabling this will reorder the products, so get active products will get the products correctly by their index.")]
        public bool AutoReorderProducts;

        protected List<TProduct> _createProducts;

        protected TProduct _createdProduct;
        private int _containerUpdated = 0;
        public static int LAYOUT_UPDATE_FRAMES = 5;
        public CanvasGroup CanvasGroup { get; protected set; }
        public LayoutGroup LayoutGroup { get; protected set; }
        public ToggleGroup ToggleGroup { get; protected set; }

        protected bool _initialized = false;

        protected virtual void Awake()
        {
            Init();
        }

        public void Init()
        {
            if (_initialized) return;

            _initialized = true;

            LayoutGroup = ProductLocation.GetComponent<LayoutGroup>();
            ToggleGroup = GetComponent<ToggleGroup>();
            CanvasGroup = GetComponent<CanvasGroup>();
            if (AutoSetProductToggleGroup && ToggleGroup == null) Debug.LogError("Cannot auto set toggle group if no toggle grup at container.");
            if (ProductLocation == null) ProductLocation = this.transform;
            if (DestroyOnStart & AddExistingProducts) Debug.LogError("Cannot destroy existing products while trying to add existing products.");
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

        protected virtual void LateUpdate()
        {
            // Update the container.
            if (_containerUpdated > 0)
            {
                _containerUpdated--;
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
            _createdProduct = Instantiate(ProductPrefab, ProductLocation);
            _createdProduct.Initialize(ActiveMode);
            _createProducts.Add(_createdProduct);
            _createdProduct.Hide();
            _createdProduct.gameObject.name = $"{_createdProduct.gameObject.name}_{_createProducts.Count}";

            if (AutoSetProductToggleGroup && ToggleGroup != null && _createdProduct.ProductToggle != null)
            {
                _createdProduct.ProductToggle.group = ToggleGroup;
            }

            if (CustomScale > 0f)
            {
                _createdProduct.transform.localScale = Vector3.one * CustomScale;
            }

            return _createdProduct;
        }



        /// <summary>
        /// Returns the next unused product. 
        /// </summary>
        public virtual TProduct GetNextProduct()
        {
            if (AutoReorderProducts) ReorderCreatedProducts();
            for (int i = 0; i < _createProducts.Count; i++)
            {
                if (!IsProductActive(_createProducts[i]))
                {
                    // if we find one, we return it
                    return _createProducts[i];
                }
            }

            // if we haven't found an inactive object (the pool is empty), and if we can extend it, we add one new object to the pool, and return it		 
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

            Debug.LogError("???");
            return false;
        }



        /// <summary>
        /// Gets a new product, new generates it
        /// </summary>
        public virtual TProduct GenerateNewProduct(TArgs args)
        {
            Init();

            TProduct product = GetNextProduct();
            product.Generate(args);

            _containerUpdated = LAYOUT_UPDATE_FRAMES;

            if (OnContentChanged != null) OnContentChanged.Invoke();

            return product;
        }



        /// <summary>
        /// Hides all previous products and generates completely new ones with the args.
        /// </summary>
        public virtual List<TProduct> SetProducts(IEnumerable<TArgs> argsList)
        {
            List<TProduct> products = new();

            DestroyAllProducts();

            foreach (TArgs args in argsList)
            {
                products.Add(GenerateNewProduct(args));
            }

            return products;
        }

        public virtual void DestroyAllProducts()
        {
            Init();

            foreach (TProduct product in _createProducts)
            {
                product.Hide();
            }

            if (OnContentChanged != null) OnContentChanged.Invoke();
        }

        /// <summary>
        /// This is very costly to do. Cache it. Do not do it every frame.
        /// </summary>
        /// <returns></returns>
        public virtual List<TProduct> GetActiveProducts()
        {
            Init();
            
            if (AutoReorderProducts) ReorderCreatedProducts();
            return _createProducts.Where(p => IsProductActive(p)).ToList();
        }

        /// <summary>
        /// Sometimes, if the elements in the conainer are reordered by other UI elements, the container will function wrongly, reordering it should fix problems
        /// </summary>
        public virtual void ReorderCreatedProducts()
        {
            Init();

            _createProducts = _createProducts.OrderBy(p => p.transform.GetSiblingIndex()).ToList();
        }

        /// <summary>
        // If Container has a layout group component, can use this function to update.
        /// </summary>
        public virtual void UpdateLayoutGroup()
        {
            if (LayoutGroup == null) return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(LayoutGroup.transform as RectTransform);
        }
    }



}
