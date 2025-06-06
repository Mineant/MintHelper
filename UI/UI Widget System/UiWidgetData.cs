using System;
using System.Collections;
using System.Collections.Generic;
using Mineant;
using UnityEngine;

namespace Mineant
{
    [System.Serializable]
    public abstract class UiWidgetData
    {
        public string Id;
        public UiPivotType PivotType;
        public Vector3 Position;
        public UiWidget OwnerUi;
        public UiWidgetCloseMode CloseMode;
        public bool IsFullScreen { get; protected set; }
        public bool AutoFitInScreen { get; protected set; }

        public abstract string GetUiWidgetId();

        public UiWidgetData()
        {
            Id = Helpers.GetUniqueID();
            PivotType = UiPivotType.TopLeft;
            Position = Vector3.zero;
            CloseMode = UiWidgetCloseMode.CloseWhenMouseClickOutside;
            OwnerUi = null;
            IsFullScreen = false;
            AutoFitInScreen = true;
        }

        /// <summary>
        /// This constructor has all the most important information of the ui widget menu that needs to be set everytime.
        /// </summary>
        /// <param name="id">The identifier for the ui widget. Used to reference the ui widget later. This is not the same as ui widget menu UI id, which is used for findding the correct ui widget ui, there might be more than one ui widget with the same ui id, but should only have one unique id.</param>
        /// <param name="pivotType"></param>
        /// <param name="position"></param>
        public UiWidgetData(string id, UiPivotType pivotType, Vector3 position)
        {
            Id = id;
            PivotType = pivotType;
            Position = position;
        }

        public TUiWidget GetUi<TUiWidget>() where TUiWidget : UiWidget
        {
            return (TUiWidget)OwnerUi;
        }

        public virtual bool IsMouseInsideUiWidget(string uiWidgetId)
        {
            UiWidget ui = UiWidgetManager.Instance.FindUiWidget(uiWidgetId);
            if (ui == null) return false;

            if (RectTransformUtility.RectangleContainsScreenPoint(ui.GetComponent<RectTransform>(), Input.mousePosition))
            {
                return true;
            }

            if (ui.BaseData.IsMouseInsideSubUiWidget())
            {
                return true;
            }

            return false;
        }

        public void EnableFullScreen()
        {
            IsFullScreen = true;
            this.Position = Vector3.zero;
            this.PivotType = UiPivotType.BottomLeft;
        }

        public void DisableAutoFitInScreen()
        {
            AutoFitInScreen = false;
        }

        public virtual bool ShouldAutoHide()
        {
            return true;
        }

        public virtual bool IsMouseInsideSubUiWidget()
        {
            return false;
        }

        public virtual void OnCreate()
        {

        }

        public virtual void OnDestroy()
        {

        }


    }


    public enum UiWidgetCloseMode
    {
        CloseWhenMouseClickOutside,
        CloseWhenClickOtherUiElements,
    }
}