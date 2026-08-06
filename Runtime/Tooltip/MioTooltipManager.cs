using System.Collections.Generic;
using MioHelper.TextFormat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MioHelper.Tooltip
{
    /// <summary>
    /// Click + hover tooltip manager. Owns a single persistent <see cref="TooltipUIProduct"/>
    /// view (no pooling). Resolves {L:keyword} link ids via <see cref="IMioTooltipProvider"/>,
    /// pre-formats content through <see cref="MioTextFormatter"/>, positions the view at the
    /// anchor point, and owns the close behaviors (close button, click-outside blocker, re-show
    /// on a new link). Place one instance in a scene (usually a persistent UI canvas).
    /// </summary>
    [AddComponentMenu("MioHelper/UI/Mio Tooltip Manager")]
    public class MioTooltipManager : Singleton<MioTooltipManager>
    {
        [Header("Display")]
        [Tooltip("TooltipUIProduct shell (NameText/DescriptionText/IconImage). Required.")]
        public TooltipUIProduct View;
        [Tooltip("Default keyword → content table. A runtime SetProvider override wins.")]
        public MioTooltipTable Table;
        [Tooltip("Styles/number formats for tooltip rich text. Null → MioTextFormatter.DefaultSettings.")]
        public MioTextSettings Settings;

        [Header("Close")]
        [Tooltip("Optional close button, wired to HideTooltip.")]
        public Button CloseButton;

        [Header("Positioning")]
        [Tooltip("Screen-space offset applied to the anchor point before placing the tooltip.")]
        public Vector2 Offset = new Vector2(12f, -12f);

        /// <summary>Current content provider; falls back to <see cref="Table"/> if none is set via <see cref="SetProvider"/>.</summary>
        public IMioTooltipProvider Provider => _provider != null ? _provider : Table;

        public bool IsShowing { get; private set; }

        private IMioTooltipProvider _provider;
        private Canvas _canvas;
        private RectTransform _canvasRect;
        private Camera _canvasCamera;
        private Button _outsideBlocker;

        protected override void AwakeSingleton()
        {
            WireUp();
        }

        protected override void OnDestroy()
        {
            if (CloseButton != null) CloseButton.onClick.RemoveListener(HideTooltip);
            base.OnDestroy();
        }

        public void SetProvider(IMioTooltipProvider provider) => _provider = provider;

        /// <summary>
        /// Runtime wiring for code-built setups (serialized fields are assigned after
        /// AddComponent). Idempotent.
        /// </summary>
        public void Initialize(TooltipUIProduct view, Button closeButton = null, MioTooltipTable table = null,
            MioTextSettings settings = null, IMioTooltipProvider provider = null)
        {
            View = view;
            CloseButton = closeButton;
            if (table != null) Table = table;
            if (settings != null) Settings = settings;
            if (provider != null) _provider = provider;
            WireUp();
        }

        /// <summary>Resolve a {L:keyword} link id and show the tooltip at a screen position.</summary>
        public void ShowTooltip(string keyword, Vector2 screenPosition, IReadOnlyDictionary<string, object> contextValues = null)
        {
            if (Provider == null)
            {
                Warn("ShowTooltip: no IMioTooltipProvider assigned (set Table or call SetProvider).");
                return;
            }
            if (!Provider.TryGetTooltip(keyword, out MioTooltipContent content))
            {
                Warn($"ShowTooltip: no tooltip content for keyword '{keyword}'.");
                return;
            }

            ShowTooltip(content, screenPosition, contextValues);
        }

        /// <summary>Show a tooltip from raw content at a screen position (no keyword lookup).</summary>
        public void ShowTooltip(MioTooltipContent content, Vector2 screenPosition, IReadOnlyDictionary<string, object> contextValues = null)
        {
            if (content == null) return;
            if (View == null)
            {
                Warn("ShowTooltip: no TooltipUIProduct view assigned.");
                return;
            }

            string name = FormatContent(content.Name, content.Values, contextValues);
            string description = FormatContent(content.Description, content.Values, contextValues);

            Vector3 world = ResolveWorldPosition(screenPosition);
            View.transform.position = world;
            View.Generate(new TooltipProductArgs(new TooltipArgs(world, content.Icon, name, description)));
            SetShowing(true);
        }

        /// <summary>Show a tooltip from prepared args (e.g. hover triggers building from IProvideTooltip).</summary>
        public void Show(TooltipArgs args)
        {
            if (View == null)
            {
                Warn("Show: no TooltipUIProduct view assigned.");
                return;
            }

            View.transform.position = args.Position;
            View.Generate(new TooltipProductArgs(args));
            SetShowing(true);
        }

        /// <summary>Show a tooltip from prepared args, converting a screen-space anchor to the
        /// view's canvas space (correct under a CanvasScaler).</summary>
        public void ShowTooltip(TooltipArgs args, Vector2 screenPosition)
        {
            if (View == null)
            {
                Warn("ShowTooltip: no TooltipUIProduct view assigned.");
                return;
            }

            Vector3 world = ResolveWorldPosition(screenPosition);
            View.transform.position = world;
            View.Generate(new TooltipProductArgs(new TooltipArgs(world, args.Icon, args.Name, args.Description)));
            SetShowing(true);
        }

        public void HideTooltip()
        {
            if (View != null) View.Hide();
            SetShowing(false);
        }

        private string FormatContent(string template, List<MioTextParameter> entryValues, IReadOnlyDictionary<string, object> contextValues)
        {
            if (string.IsNullOrEmpty(template)) return template;
            MioTextSettings settings = Settings != null ? Settings : MioTextFormatter.DefaultSettings;
            return MioTextFormatter.Format(template, BuildValues(entryValues, contextValues), settings);
        }

        private static Dictionary<string, object> BuildValues(List<MioTextParameter> entryValues, IReadOnlyDictionary<string, object> contextValues)
        {
            var values = new Dictionary<string, object>();
            if (contextValues != null)
            {
                foreach (var kvp in contextValues) values[kvp.Key] = kvp.Value;
            }
            if (entryValues != null)
            {
                foreach (MioTextParameter parameter in entryValues)
                {
                    if (parameter != null && !string.IsNullOrEmpty(parameter.Key)) values[parameter.Key] = parameter.Value;
                }
            }
            return values;
        }

        private void WireUp()
        {
            if (CloseButton != null)
            {
                CloseButton.onClick.RemoveListener(HideTooltip);
                CloseButton.onClick.AddListener(HideTooltip);
            }

            if (View != null)
            {
                EnsureNestedLink(View.NameText);
                EnsureNestedLink(View.DescriptionText);
            }

            BuildOutsideBlocker();
        }

        /// <summary>Makes {L:} links inside tooltip text clickable too (they re-show the tooltip for their keyword).</summary>
        private void EnsureNestedLink(TMP_Text text)
        {
            if (text == null) return;
            if (!text.TryGetComponent(out MioTooltipLink link))
            {
                link = text.gameObject.AddComponent<MioTooltipLink>();
            }
            link.SetManager(this);
        }

        private void SetShowing(bool value)
        {
            IsShowing = value;
            if (value && _outsideBlocker == null) BuildOutsideBlocker();
            if (_outsideBlocker != null) _outsideBlocker.gameObject.SetActive(value);
        }

        /// <summary>Full-canvas invisible button that closes the tooltip on any click outside it.</summary>
        private void BuildOutsideBlocker()
        {
            if (_outsideBlocker != null) return;
            ResolveCanvas();
            if (_canvasRect == null) return;

            var go = new GameObject("Tooltip Outside Blocker", typeof(Image), typeof(Button));
            go.transform.SetParent(_canvasRect, false);
            go.transform.SetAsFirstSibling();

            var image = go.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;

            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(HideTooltip);

            _outsideBlocker = button;
            _outsideBlocker.gameObject.SetActive(IsShowing);
        }

        private Vector3 ResolveWorldPosition(Vector2 screenPosition)
        {
            ResolveCanvas();
            Vector2 point = screenPosition + Offset;
            if (_canvasRect != null
                && RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRect, point, _canvasCamera, out Vector3 world))
            {
                return world;
            }
            return point;
        }

        private void ResolveCanvas()
        {
            if (_canvas != null) return;
            if (View == null) return;

            _canvas = View.GetComponentInParent<Canvas>();
            if (_canvas == null) return;

            _canvasRect = _canvas.transform as RectTransform;
            _canvasCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : (_canvas.worldCamera != null ? _canvas.worldCamera : Camera.main);
        }

        private static void Warn(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[MioTooltipManager] {message}");
#endif
        }
    }
}
