using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mineant
{
    public class UICellScroller : MonoBehaviour
    {
        protected ScrollRect _scrollRect;
        protected Container _container;
        protected List<UICell> _products;
        protected float _width;
        protected Vector3 _startPosition;
        protected Vector3 _position;
        void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _container = GetComponentInChildren<Container>();

            Vector3[] corners = new Vector3[4];
            GetComponent<RectTransform>().GetWorldCorners(corners);
            _width = corners[3].x - corners[0].x;   // Bottom right - bottom left

            _position = GetComponent<RectTransform>().position;
            _startPosition = new Vector3(_position.x - _width / 2, _position.y, _position.z);
            _container.OnContentChanged += OnContainerContentChanged;
            OnContainerContentChanged();
            _scrollRect.onValueChanged.AddListener(OnScrollRectChanged);
        }

        private void OnContainerContentChanged()
        {
            _products = new List<UICell>();
            foreach (Transform child in _container.transform)
            {
                _products.Add(child.GetComponent<UICell>());
            }
        }

        private void OnScrollRectChanged(Vector2 arg0)
        {
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
