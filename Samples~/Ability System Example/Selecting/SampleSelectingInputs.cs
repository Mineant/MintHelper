using UnityEngine;

namespace MioHelper.AbilitySystem.Samples
{
    /// <summary>
    /// Sample mouse-based selecting input. Reads from Unity's mouse position and
    /// Z/X keys for confirm/cancel.
    /// Replace with your own input system (Unity Input System, Rewired, etc.).
    /// </summary>
    public class MouseSelectingInput : ISelectingInput
    {
        public Vector2 GetTargetingPosition()
        {
            Vector2 mousePos = Input.mousePosition;
            return Camera.main != null ? (Vector2)Camera.main.ScreenToWorldPoint(mousePos) : Vector2.zero;
        }

        public bool GetConfirmInput() => Input.GetKeyDown(KeyCode.Z);
        public bool GetCancelInput() => Input.GetKeyDown(KeyCode.X);
    }

    /// <summary>
    /// Sample Gizmo-based selecting visual. Draws wire spheres and shapes in the Scene view.
    /// Replace with your own visual system (decals, shaders, UI overlays).
    /// </summary>
    public class GizmoSelectingVisual : MonoBehaviour, ISelectingVisual
    {
        private bool _isShowing;
        private SelectionInfo _currentSelection;
        private RuntimeAbilitySelector _selectorParams;
        private Vector3 _aimPosition;
        private Vector3 _aimDirection;

        public void ShowTargeting(SelectionInfo selectionInfo, RuntimeAbilitySelector parameters,
            Vector3 aimPosition, Vector3 aimDirection)
        {
            _currentSelection = selectionInfo;
            _selectorParams = parameters;
            _aimPosition = aimPosition;
            _aimDirection = aimDirection;
            _isShowing = true;
        }

        public void HideTargeting()
        {
            _isShowing = false;
        }

        private void OnDrawGizmos()
        {
            if (!_isShowing || _selectorParams?.OwnerAbilityComponent == null) return;

            var config = _selectorParams.OwnerAbilityComponent;
            Vector3 origin = transform.position;

            Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
            Gizmos.DrawWireSphere(origin, config.Distance);

            switch (config.SelectingType)
            {
                case SelectingType.Circle:
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(_aimPosition, config.Range);
                    break;
                case SelectingType.Rect:
                    Gizmos.color = Color.red;
                    Vector3 forward = _aimDirection.normalized;
                    Vector3 right = Vector3.Cross(forward, Vector3.forward).normalized;
                    float halfW = config.Range / 2;
                    Vector3 bl = origin + right * halfW;
                    Vector3 br = origin - right * halfW;
                    Vector3 fl = origin + forward * config.Distance + right * halfW;
                    Vector3 fr = origin + forward * config.Distance - right * halfW;
                    Gizmos.DrawLine(bl, fl);
                    Gizmos.DrawLine(br, fr);
                    Gizmos.DrawLine(bl, br);
                    Gizmos.DrawLine(fl, fr);
                    break;
                case SelectingType.Cone:
                    Gizmos.color = Color.red;
                    float halfAngle = config.Range / 2;
                    Vector3 left = Quaternion.Euler(0, 0, halfAngle) * _aimDirection.normalized * config.Distance;
                    Vector3 right2 = Quaternion.Euler(0, 0, -halfAngle) * _aimDirection.normalized * config.Distance;
                    Gizmos.DrawLine(origin, origin + left);
                    Gizmos.DrawLine(origin, origin + right2);
                    break;
                case SelectingType.WholeRange:
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(origin, config.Distance);
                    break;
                case SelectingType.Single:
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(_aimPosition, config.Range);
                    Gizmos.DrawLine(origin, _aimPosition);
                    break;
            }

            if (_currentSelection?.Targets != null)
            {
                Gizmos.color = Color.green;
                foreach (var target in _currentSelection.Targets)
                {
                    if (target?.Transform != null)
                        Gizmos.DrawWireSphere(target.Transform.position, 0.5f);
                }
            }
        }
    }
}
