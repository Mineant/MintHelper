using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MioHelper
{
    public class UICellScroller : MonoBehaviour
    {
        protected ScrollRect _scrollRect;
        protected List<UICell> _products;
        protected float _width;
        protected Vector3 _startPosition;
        protected Vector3 _position;

        void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();

            Vector3[] corners = new Vector3[4];
            GetComponent<RectTransform>().GetWorldCorners(corners);
            _width = corners[3].x - corners[0].x;

            _position = GetComponent<RectTransform>().position;
            _startPosition = new Vector3(_position.x - _width / 2, _position.y, _position.z);

            _scrollRect.onValueChanged.AddListener(OnScrollRectChanged);
        }

        /// <summary>
        /// Call this when the content changes to refresh the cell list.
        /// Wire to Container.ContentChanged or call manually.
        /// </summary>
        public void Refresh()
        {
            _products = new List<UICell>();
            foreach (Transform child in _scrollRect.content)
            {
                var cell = child.GetComponent<UICell>();
                if (cell != null) _products.Add(cell);
            }
        }

        private void OnScrollRectChanged(Vector2 arg0)
        {
            if (_products == null) return;
            foreach (UICell cell in _products)
            {
                Vector3 cellPosition = cell.GetComponent<RectTransform>().position;
                float distanceToStart = cellPosition.x - _startPosition.x;
                float percentage = Mathf.Clamp(distanceToStart / _width, 0f, 1f);
                cell.UpdatePosition(percentage);
            }
        }
    }
}
