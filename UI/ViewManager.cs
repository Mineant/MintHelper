using System.Collections.Generic;
using UnityEngine;

namespace Mineant
{

    public enum ViewMode
    {
        Single,
        Additive,
    }

    public sealed class ViewManager : MonoBehaviour
    {
        public static ViewManager Instance { get; private set; }

        [SerializeField]
        private bool autoInitialize;

        [SerializeField]
        private View[] views;

        [SerializeField]
        private View[] defaultView;


        // private List<View[]> _viewHistory;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (autoInitialize) Initialize();
        }

        public void Initialize()
        {
            foreach (View view in views)
            {
                view.Initialize();

                view.Hide();
            }


            if (defaultView != null)
            {
                for (int i = 0; i < defaultView.Length; i++)
                {
                    defaultView[i].ShowNoArgs();
                }
            }
        }

        public void Show<TView>(object[] args = null, ViewMode mode = ViewMode.Single) where TView : View
        {
            if (mode == ViewMode.Single) HideAll();

            foreach (View view in views)
            {
                if (view is TView)
                {
                    view.Show(args);
                }
            }
        }

        public void Show(View view, object[] args = null, ViewMode mode = ViewMode.Single)
        {
            if (mode == ViewMode.Single) HideAll();

            view.Show(args);
        }

        public void Hide<TView>()
        {
            foreach (View view in views)
            {
                if (view is TView)
                {
                    view.Hide();
                }
            }
        }

        public void HideAll()
        {
            foreach (View view in views) if (view.IsShowing()) view.Hide();
        }

        public TView GetView<TView>() where TView : View
        {
            foreach (View view in views)
            {
                if (view is TView)
                {
                    return view as TView;
                }
            }
            return null;
        }
    }

}