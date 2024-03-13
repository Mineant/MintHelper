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

        public int DefaultSize = 20;

        [Tooltip("Destroys the gameObjects under the targetted transform.")]
        public bool DestroyOnStart = false;

        public bool AutoUpdateLayout;
        public bool AddExistingProducts;

        [Header("Helperful")]
        [Tooltip("if a toggle group can be found on this object, then if products hv toggle group, will automatically assign.")]
        public bool AutoSetProductToggleGroup;

        [Tooltip("Will set all product to this scale if > 0f.")]
        public float CustomScale = -1f;

        [Tooltip("If other scripts will reorder the products inside this container, for example, moave the products to another parent, or change the sibling index, enabling this will reorder the products, so get active products will get the products correctly by their index.")]
        public bool AutoReorderProducts;

        // public List<TProduct> CreatedProducts
        // {
        //     get
        //     { return _createProducts; }
        //     set
        //     {
        //         _createProducts = value;
        //     }
        // }

        protected List<TProduct> _createProducts;

        protected TProduct _createdProduct;
        protected LayoutGroup _layoutGroup;
        private int _containerUpdated = 0;
        public static int LAYOUT_UPDATE_FRAMES = 5;
        public ToggleGroup ToggleGroup { get; protected set; }


        void Awake()
        {
            _layoutGroup = ProductLocation.GetComponent<LayoutGroup>();
            ToggleGroup = GetComponent<ToggleGroup>();
            if (AutoSetProductToggleGroup && ToggleGroup == null) Debug.LogError("Cannot auto set toggle group if no toggle grup at container.");
            if (ProductLocation == null) ProductLocation = this.transform;
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

        void LateUpdate()
        {
            if (_containerUpdated > 0)
            {
                _containerUpdated--;
                if (AutoUpdateLayout) UpdateLayoutGroup();
            }
        }



        protected void CreatePool()
        {
            for (int i = 0; i < DefaultSize; i++)
            {
                CreateNewProduct();
            }
        }

        protected TProduct CreateNewProduct()
        {
            _createdProduct = Instantiate(ProductPrefab, ProductLocation);
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
        public TProduct GetNextProduct()
        {
            if (AutoReorderProducts) ReorderCreatedProducts();
            for (int i = 0; i < _createProducts.Count; i++)
            {
                if (!_createProducts[i].gameObject.activeSelf)
                {
                    // if we find one, we return it
                    return _createProducts[i];
                }
            }

            // if we haven't found an inactive object (the pool is empty), and if we can extend it, we add one new object to the pool, and return it		 
            return CreateNewProduct();
        }

        /// <summary>
        /// Gets a new product, new generates it
        /// </summary>
        public virtual TProduct GenerateNewProduct(TArgs args)
        {
            TProduct product = GetNextProduct();
            product.Generate(args);

            _containerUpdated = LAYOUT_UPDATE_FRAMES;

            if (OnContentChanged != null) OnContentChanged.Invoke();

            return product;
        }



        /// <summary>
        /// Hides all previous products and generates completely new ones with the args.
        /// </summary>
        public void NewProductBatch(IEnumerable<TArgs> argsList)
        {
            DestroyAllProducts();

            foreach (TArgs args in argsList)
            {
                GenerateNewProduct(args);
            }
        }

        public virtual void DestroyAllProducts()
        {
            foreach (TProduct product in _createProducts)
            {
                product.Hide();
            }

            if (OnContentChanged != null) OnContentChanged.Invoke();
        }

        public List<TProduct> GetActiveProducts()
        {
            if (AutoReorderProducts) ReorderCreatedProducts();
            return _createProducts.Where(p => p.gameObject.activeSelf).ToList();
        }

        /// <summary>
        /// Sometimes, if the elements in the conainer are reordered by other UI elements, the container will function wrongly, reordering it should fix problems
        /// </summary>
        public void ReorderCreatedProducts()
        {
            _createProducts = _createProducts.OrderBy(p => p.transform.GetSiblingIndex()).ToList();
        }

        /// <summary>
        // If Container has a layout group component, can use this function to update.
        /// </summary>
        public void UpdateLayoutGroup()
        {
            if (_layoutGroup == null) return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroup.transform as RectTransform);
        }
    }



}
