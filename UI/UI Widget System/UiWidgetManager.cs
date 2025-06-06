using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Mineant.UI
{
    public class UiWidgetManager : Singleton<UiWidgetManager>
    {
        public enum UiWidgetPrefabStorageMode { ResourceFolder, CustomMethod = 100 }

        [Tooltip("Where should the UI be spawned by default. For ResourceFolder, the Id would be the prefab name.")]
        public RectTransform DefaultWidgetParent;

        [Tooltip("Where should we find the prefabs for the ui widgets")]
        public UiWidgetPrefabStorageMode WidgetPrefabStorageMode;

        [Tooltip("If use resource folder, specify the folder name here.")]
        public string PrefabResourceFolder = "Ui Widget Prefab";

        [Tooltip("This will check for what UI is the user clicking. When you use 'close UI when click other UI', will need this functionality. But this has performance impact.")]
        public bool EnableUiClickCheck;

        public Action<UiWidgetData> OnShowWidget;
        public Action<UiWidgetData> OnHideWidget;
        public Action<GameObject> OnClickedUI;
        protected Dictionary<string, UiWidget> _widgetDatabase;

        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();

            _widgetDatabase = new();
        }

        protected virtual void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
            {
                CheckUI(Input.mousePosition);
            }
#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            CheckUI(Input.GetTouch(0).position);
        }
#elif UNITY_XBOXONE || UNITY_PS4 || UNITY_PS5 
        if (Input.GetButtonDown("Submit")) // Detects "A" on Xbox or "X" on PlayStation
        {
            GameObject selectedUI = EventSystem.current.currentSelectedGameObject;
            if (selectedUI != null)
            {
                Debug.Log("Selected UI: " + selectedUI.name);
            }
        }
#endif
        }

        protected virtual void CheckUI(Vector2 inputPosition)
        {
            if (!EnableUiClickCheck) return;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = inputPosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                OnClickedUI?.Invoke(result.gameObject);
                return;
            }
        }

        public UiWidget ShowUiWidget(UiWidgetData data, RectTransform parent = null)
        {
            if (_widgetDatabase.TryGetValue(data.Id, out UiWidget existingUiWidget))
            {
                Debug.LogWarning("Ui Widget with id " + data.Id + " already exists.");
                return existingUiWidget;
            }

            if (parent == null)
            {
                parent = DefaultWidgetParent;
            }

            // Spawn the Ui Widget
            UiWidget widgetPrefab = FindUiWidgetPrefab(data.GetUiWidgetId());
            UiWidget widgetInstance = Instantiate(widgetPrefab, parent);
            RectTransform widgetTransform = widgetInstance.GetComponent<RectTransform>();

            Vector2 pivot = data.PivotType.GetPivotPosition();

            widgetTransform.pivot = pivot;
            widgetTransform.position = data.Position;

            widgetInstance.Init(data);
            data.OnCreate();

            AutoFitInScreen(widgetInstance);

            // Set the dictionary
            _widgetDatabase[data.Id] = widgetInstance;

            OnShowWidget?.Invoke(data);

            return widgetInstance;
        }

        public void AutoFitInScreen(UiWidget widgetInstance)
        {
            RectTransform rectTransform = widgetInstance.GetComponent<RectTransform>();
            UiWidgetData data = widgetInstance.BaseData;

            // Check if need to auto fit in screen
            if (!data.IsFullScreen && data.AutoFitInScreen)
            {
                AudoFitUiInScreen(rectTransform);
            }
        }


        public void HideUiWidget(string id)
        {
            if (!_widgetDatabase.ContainsKey(id))
            {
                Debug.Log("Ui Widget with id " + id + " does not exist.");
                return;
            }

            UiWidget widget = _widgetDatabase[id];
            widget.BaseData.OnDestroy();
            widget.OnDestroy();
            Destroy(widget.gameObject);
            _widgetDatabase.Remove(id);

            OnHideWidget?.Invoke(widget.BaseData);
        }

        public bool UiWidgetExists(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return _widgetDatabase.ContainsKey(id);
        }

        public UiWidget FindUiWidget(string id)
        {
            if (UiWidgetExists(id))
            {
                return _widgetDatabase[id];
            }
            return null;
        }

        /// <summary>
        /// For resource mode, the ID would tbe the prefab's name.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <returns></returns>
        public UiWidget FindUiWidgetPrefab(string widgetId)
        {
            switch (WidgetPrefabStorageMode)
            {
                case UiWidgetPrefabStorageMode.ResourceFolder:
                    {
                        GameObject go = Resources.Load<GameObject>(Path.Combine(PrefabResourceFolder, widgetId));
                        if (go == null)
                        {
                            Debug.LogError("Cannot find prefab with id " + widgetId + " in resource folder " + PrefabResourceFolder);
                            return null;
                        }
                        if (!go.TryGetComponent<UiWidget>(out UiWidget ui))
                        {
                            Debug.LogError("Prefab with id " + widgetId + " does not have a Ui Widget component.");
                            return null;
                        }
                        return ui;
                    }
                case UiWidgetPrefabStorageMode.CustomMethod:
                    return CustomMethodFindUiWidgetPrefab(widgetId);
                default:
                    Debug.LogError("Invalid WidgetPrefabStorageMode.");
                    return null;
            }
        }

        public virtual UiWidget CustomMethodFindUiWidgetPrefab(string widgetId)
        {
            Debug.LogError("CustomMethodFindUiWidgetPrefab not implemented.");
            return null;
        }

        public void AudoFitUiInScreen(RectTransform rectTransform)
        {

            // Check if the widget is outside the screen
            Vector3[] screenCorners = new Vector3[4];
            rectTransform.GetWorldCorners(screenCorners);
            bool isOutsideScreen = false;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (Vector3 screenCorner in screenCorners)
            {
                if (screenCorner.x < minX) minX = screenCorner.x;
                if (screenCorner.x > maxX) maxX = screenCorner.x;
                if (screenCorner.y < minY) minY = screenCorner.y;
                if (screenCorner.y > maxY) maxY = screenCorner.y;

                if (screenCorner.x < 0 || screenCorner.x > Screen.width ||
                    screenCorner.y < 0 || screenCorner.y > Screen.height)
                {
                    isOutsideScreen = true;
                }
            }

            if (isOutsideScreen)
            {
                // Calculate the necessary offsets
                float offsetX = 0;
                float offsetY = 0;

                if (minX < 0)
                {
                    offsetX = -minX;
                }
                if (maxX > Screen.width)
                {
                    offsetX = Screen.width - maxX;
                }
                if (minY < 0)
                {
                    offsetY = -minY;
                }
                if (maxY > Screen.height)
                {
                    offsetY = Screen.height - maxY;
                }

                // Apply the offset to move the ui widget inside the screen boundaries
                rectTransform.anchoredPosition += new Vector2(offsetX, offsetY);

            }
        }
    }
}