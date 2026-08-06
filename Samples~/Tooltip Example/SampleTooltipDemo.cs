using System.Collections.Generic;
using MioHelper.TextFormat;
using MioHelper.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MioHelper.Samples.Tooltip
{
    /// <summary>
    /// Self-contained demo of the click + hover tooltip module — no scene assets needed. Builds
    /// a canvas, EventSystem, runtime MioTextSettings + MioTooltipTable, body text with {L:}
    /// links, a hoverable panel, and a TooltipUIProduct view + MioTooltipManager at runtime.
    /// Attach to any GameObject and press Play.
    ///
    /// What to try:
    ///  - Click 查看劍 / 查看盾 / (nested) 盾牌 / 藥水 to open tooltips near the click.
    ///  - Hover the grey panel to see a hover tooltip.
    ///  - Close a tooltip with the 關閉 button or by clicking empty space.
    /// </summary>
    public class SampleTooltipDemo : MonoBehaviour
    {
        private void Awake() => BuildDemo();

        private static void BuildDemo()
        {
            EnsureEventSystem();

            MioTextSettings settings = BuildSettings();
            MioTooltipTable table = BuildTable();

            var canvasGo = new GameObject("Tooltip Sample Canvas");
            DontDestroyOnLoad(canvasGo);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            var canvasRect = (RectTransform)canvasGo.transform;

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
                Debug.LogWarning("[Tooltip Sample] No default TMP font — import TMP Essentials or set one in TMP Settings for the text to be visible.");

            // --- Body text with clickable {L:} links ---
            TMP_Text body = AddText(canvasRect, "BodyText", font,
                "點擊 {C:focus}{L:item_sword}查看劍{/L}{/C} 或 {C:focus}{L:item_shield}查看盾{/L}{/C} 的詳情。",
                new Vector2(0f, 170f), 720f, TextAlignmentOptions.TopLeft, 20f);
            MioTooltipLink bodyLink = body.gameObject.AddComponent<MioTooltipLink>();

            // --- Hoverable panel (demos hover tooltip via MioTooltipHoverTrigger + static content) ---
            RectTransform hoverPanel = AddPanel(canvasRect, "Hover Panel", new Vector2(0f, 40f),
                new Vector2(280f, 56f), new Color(0.15f, 0.17f, 0.22f, 1f));
            AddText(hoverPanel, "HoverLabel", font, "把滑鼠移到我身上 (Hover me)", Vector2.zero, 280f, TextAlignmentOptions.Center, 16f);
            var hoverTrigger = hoverPanel.gameObject.AddComponent<MioTooltipHoverTrigger>();
            hoverTrigger.Content = new MioTooltipContent
            {
                Keyword = "hover_info",
                Name = "懸停提示",
                Description = "這是 {C:rare}稀有{/C} 的懸停 tooltip，也支援 @{hp} 數值。",
                Values = new List<MioTextParameter> { new MioTextParameter { Key = "hp", Value = "150" } },
            };

            // --- Tooltip view (hidden until a trigger shows it) ---
            BuildTooltipView(canvasRect, font, out TooltipUIProduct view, out Button closeButton);

            // --- Manager ---
            var managerGo = new GameObject("MioTooltipManager");
            var manager = managerGo.AddComponent<MioTooltipManager>();
            manager.Initialize(view, closeButton, table, settings);

            // Route the body links to the manager.
            bodyLink.SetManager(manager);
        }

        /// <summary>Scene-less samples get no default EventSystem; pointer events need one.</summary>
        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static MioTextSettings BuildSettings()
        {
            var settings = ScriptableObject.CreateInstance<MioTextSettings>();
            settings.Styles = new List<MioTextStyle>
            {
                new MioTextStyle { Name = "buff",   Color = new Color(0.35f, 0.90f, 0.40f), IsBold = true },
                new MioTextStyle { Name = "damage", Color = new Color(1.00f, 0.35f, 0.35f), IsBold = true },
                new MioTextStyle { Name = "heal",   Color = new Color(0.40f, 0.90f, 0.45f) },
                new MioTextStyle { Name = "rare",   Color = new Color(0.95f, 0.80f, 0.35f), IsBold = true },
                new MioTextStyle { Name = "focus",  Color = new Color(1.00f, 0.85f, 0.20f) },
            };
            return settings;
        }

        private static MioTooltipTable BuildTable()
        {
            var table = ScriptableObject.CreateInstance<MioTooltipTable>();

            table.SetContent(new MioTooltipContent
            {
                Keyword = "item_sword",
                Name = "鋼鐵劍",
                Description = "一把 {C:rare}稀有{/C} 的劍。\n耐久 {N:int}@{durability}{/N}。\n詳情見 {L:item_shield}盾牌{/L}。",
                Values = new List<MioTextParameter> { new MioTextParameter { Key = "durability", Value = "120" } },
            });
            table.SetContent(new MioTooltipContent
            {
                Keyword = "item_shield",
                Name = "鋼鐵盾",
                Description = "防禦 +{N:int}@{defense}{/N}。\n詳情見 {L:item_potion}藥水{/L}。",
                Values = new List<MioTextParameter> { new MioTextParameter { Key = "defense", Value = "15" } },
            });
            table.SetContent(new MioTooltipContent
            {
                Keyword = "item_potion",
                Name = "回復藥水",
                Description = "回復 {C:heal}@{heal}{/C} 點 HP。",
                Values = new List<MioTextParameter> { new MioTextParameter { Key = "heal", Value = "50" } },
            });

            return table;
        }

        /// <summary>A fixed-width auto-height panel with a close button, reused as the TooltipUIProduct view.</summary>
        private static void BuildTooltipView(RectTransform parent, TMP_FontAsset font, out TooltipUIProduct view, out Button closeButton)
        {
            var go = new GameObject("Tooltip View", typeof(RectTransform), typeof(Image),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(TooltipUIProduct));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.pivot = new Vector2(0f, 1f); // top-left pivot → hangs below-right of the anchor point
            rect.sizeDelta = new Vector2(280f, 0f);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.10f, 0.11f, 0.14f, 0.96f);
            image.raycastTarget = true;

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 10, 10);
            layout.spacing = 6f;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var fitter = go.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            view = go.GetComponent<TooltipUIProduct>();

            view.NameText = AddText(rect, "NameText", font, "Name", new Vector2(0f, 0f), 256f, TextAlignmentOptions.Left, 20f);
            view.NameText.fontStyle = FontStyles.Bold;
            view.DescriptionText = AddText(rect, "DescriptionText", font, "Description", new Vector2(0f, 0f), 256f, TextAlignmentOptions.Left, 16f);
            view.DescriptionText.enableWordWrapping = true;

            // Close button: fixed 60x28 via LayoutElement so the vertical layout keeps its size.
            var closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            closeGo.transform.SetParent(rect, false);
            var closeRect = (RectTransform)closeGo.transform;
            closeRect.sizeDelta = new Vector2(60f, 28f);
            closeGo.GetComponent<LayoutElement>().preferredWidth = 60f;
            closeGo.GetComponent<LayoutElement>().preferredHeight = 28f;
            closeGo.GetComponent<Image>().color = new Color(0.35f, 0.10f, 0.10f, 1f);
            closeButton = closeGo.GetComponent<Button>();
            AddText(closeRect, "Label", font, "關閉", new Vector2(0f, 0f), 60f, TextAlignmentOptions.Center, 14f);

            go.SetActive(false); // the manager activates it
        }

        private static TMP_Text AddText(Transform parent, string name, TMP_FontAsset font, string content,
            Vector2 anchoredPos, float width, TextAlignmentOptions align, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(width, 0f);
            rt.anchoredPosition = anchoredPos;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;
            tmp.color = Color.white;
            tmp.text = content;
            return tmp;
        }

        private static RectTransform AddPanel(RectTransform parent, string name, Vector2 anchoredPos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            go.GetComponent<Image>().color = color;
            return rt;
        }
    }
}
