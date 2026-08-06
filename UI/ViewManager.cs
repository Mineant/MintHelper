using System;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper
{

    public enum ViewMode
    {
        Single,
        Additive,
    }

    [DefaultExecutionOrder(-100)]
    public sealed class ViewManager : MonoBehaviour
    {
        public static ViewManager Instance { get; private set; }

        [SerializeField]
        private bool autoInitialize;

        [SerializeField]
        private View[] views;

        [SerializeField]
        private View[] defaultView;

        // Type-index of the serialized `views` array, built once in Awake so
        // per-type queries are O(distinct view types) instead of O(all views).
        // Keyed by exact runtime type; GetViews resolves base-type queries via
        // assignability, preserving the old `view is TView` scan semantics.
        private readonly Dictionary<Type, List<View>> _viewsByType = new Dictionary<Type, List<View>>();

        private void Awake()
        {
            Instance = this;
            RebuildRegistry();
        }

        private void RebuildRegistry()
        {
            _viewsByType.Clear();
            if (views == null) return;

            foreach (View view in views)
            {
                if (view == null) continue;

                Type type = view.GetType();
                if (!_viewsByType.TryGetValue(type, out List<View> list))
                {
                    list = new List<View>();
                    _viewsByType.Add(type, list);
                }
                list.Add(view);
            }
        }

        // All registered views assignable to `type`, in registration order.
        private IEnumerable<View> GetViews(Type type)
        {
            foreach (KeyValuePair<Type, List<View>> group in _viewsByType)
            {
                if (type.IsAssignableFrom(group.Key))
                {
                    foreach (View view in group.Value) yield return view;
                }
            }
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

            foreach (View view in GetViews(typeof(TView)))
            {
                view.Show(args);
            }
        }

        public void Show(View view, object[] args = null, ViewMode mode = ViewMode.Single)
        {
            if (mode == ViewMode.Single) HideAll();

            view.Show(args);
        }

        public void Hide<TView>()
        {
            foreach (View view in GetViews(typeof(TView)))
            {
                view.Hide();
            }
        }

        public void HideAll()
        {
            foreach (View view in views) if (view.IsShowing()) view.Hide();
        }

        public TView GetView<TView>() where TView : View
        {
            foreach (View view in GetViews(typeof(TView)))
            {
                return view as TView;
            }
            return null;
        }
    }

}
