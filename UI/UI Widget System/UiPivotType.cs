using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant
{
    public enum UiPivotType
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,

        Custom = 100,
    }


    public static class UiPivotExtensions
    {
        public static Vector2 GetPivotPosition(this UiPivotType pivotType)
        {
            Vector2 pivot = Vector2.zero;

            switch (pivotType)
            {
                case UiPivotType.TopLeft:
                    pivot = new Vector2(0, 1);
                    break;
                case UiPivotType.TopCenter:
                    pivot = new Vector2(0.5f, 1);
                    break;
                case UiPivotType.TopRight:
                    pivot = new Vector2(1, 1);
                    break;
                case UiPivotType.MiddleLeft:
                    pivot = new Vector2(0, 0.5f);
                    break;
                case UiPivotType.MiddleCenter:
                    pivot = new Vector2(0.5f, 0.5f);
                    break;
                case UiPivotType.MiddleRight:
                    pivot = new Vector2(1, 0.5f);
                    break;
                case UiPivotType.BottomLeft:
                    pivot = new Vector2(0, 0);
                    break;
                case UiPivotType.BottomCenter:
                    pivot = new Vector2(0.5f, 0);
                    break;
                case UiPivotType.BottomRight:
                    pivot = new Vector2(1, 0);
                    break;
                case UiPivotType.Custom:
                    Debug.LogError("We do not support custom here.");
                    break;
                default:
                    Debug.LogError("Invalid UiPivotType.");
                    break;
            }

            return pivot;
        }
    }
}