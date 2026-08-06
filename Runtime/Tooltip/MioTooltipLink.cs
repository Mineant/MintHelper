using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MioHelper.Tooltip
{
    /// <summary>
    /// Click detection for {L:keyword} TMP links on the attached TextMeshProUGUI. Fires
    /// <see cref="LinkClicked"/> on a link hit and, when a manager is known, auto-routes to
    /// <see cref="MioTooltipManager.ShowTooltip"/>. Camera resolution mirrors TMP: overlay → null;
    /// camera-space → canvas.worldCamera.
    /// </summary>
    [AddComponentMenu("MioHelper/UI/Mio Tooltip Link")]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class MioTooltipLink : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>Fired whenever a {L:keyword} link is clicked: (keyword, screenPosition).</summary>
        public event Action<string, Vector2> LinkClicked;

        [SerializeField] private TextMeshProUGUI _text;
        [Tooltip("Manager routed to on link click. Null → MioTooltipManager.Instance.")]
        [SerializeField] private MioTooltipManager _manager;
        [Tooltip("Camera used for link hit-testing. Null → resolved from the canvas.")]
        [SerializeField] private Camera _camera;

        private void Awake()
        {
            if (_text == null) _text = GetComponent<TextMeshProUGUI>();
            if (_manager == null) _manager = MioTooltipManager.Instance;
        }

        /// <summary>Bind (or rebind) the manager this link routes to. Idempotent.</summary>
        public void SetManager(MioTooltipManager manager)
        {
            _manager = manager;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_text == null) return;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, eventData.position, ResolveCamera());
            if (linkIndex == -1) return;

            string keyword = _text.textInfo.linkInfo[linkIndex].GetLinkID();
            LinkClicked?.Invoke(keyword, eventData.position);
            if (_manager != null) _manager.ShowTooltip(keyword, eventData.position);
        }

        private Camera ResolveCamera()
        {
            if (_camera != null) return _camera;

            Canvas canvas = _text != null ? _text.GetComponentInParent<Canvas>() : null;
            if (canvas == null) return Camera.main;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
            return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }
    }
}
