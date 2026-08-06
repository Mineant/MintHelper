using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MioHelper.TextFormat;

namespace MioHelper.Samples.TextFormat
{
    /// <summary>
    /// Self-contained demo of the TextFormat module — no scene assets needed. Builds a canvas
    /// and a MioTextSettings asset at runtime, then renders every grammar feature as a raw
    /// template next to its formatted output. Attach to any GameObject in a scene and press Play.
    ///
    /// Unresolvable references pass through verbatim and log warnings in the console (dev
    /// builds only), so a broken template is always visible in the output, never silent.
    /// </summary>
    public class SampleTextFormatDemo : MonoBehaviour
    {
        private void Awake() => BuildDemo();

        private static void BuildDemo()
        {
            MioTextSettings settings = BuildSettings();

            var canvasGo = new GameObject("MioText Sample Canvas");
            DontDestroyOnLoad(canvasGo);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
                Debug.LogWarning("[MioText Sample] No default TMP font — import TMP Essentials or set one in TMP Settings for the text to be visible.");

            var values = new Dictionary<string, object>
            {
                ["dmg"] = 3,
                ["damage"] = 25f,
                ["healPct"] = 0.35f,
                ["chance"] = 0.08f,
                ["price"] = 120,
                ["tier"] = "{C:damage}SSR{/C}", // values may contain template tags (re-entrant)
            };

            AddText(canvas.transform, font, "RAW TEMPLATE", -24f, 32f, 620f, TextAlignmentOptions.TopLeft, true);
            AddText(canvas.transform, font, "FORMATTED OUTPUT", -24f, 672f, 680f, TextAlignmentOptions.TopLeft, true);

            float y = -56f;
            foreach (string template in Rows)
            {
                AddText(canvas.transform, font, template, y, 32f, 620f, TextAlignmentOptions.TopLeft, true);
                AddText(canvas.transform, font, MioTextFormatter.Format(template, values, settings), y, 672f, 680f, TextAlignmentOptions.TopLeft, false);
                y -= 36f;
            }
        }

        private static readonly string[] Rows =
        {
            "Placeholder + int        →  普攻連射+@{dmg:int} 段",
            "Style span               →  傷害 {C:damage}@{damage}{/C}",
            "N wraps placeholder       →  回復量 {C:heal}{N:pct}@{healPct}{/N}{/C}",
            "Format hint in style      →  暴擊機率 {C:focus}@{chance:pct+}{/C}",
            "Money literal             →  售價 {N:money}100{/N}",
            "Mult literal              →  倍率 {N:mult}2.5{/N}",
            "Link                      →  點擊 {L:item_sword}查看詳情{/L}",
            "Control key (line break)  →  第一行@{br}第二行",
            "Re-entrant value          →  品質 {C:focus}@{tier}{/C}",
            "S alias of C              →  標題 {S:buff}BUFF{/S}",
            "Case-tolerant close       →  重點 {C:focus}黃字{/c}",
            "Escapes                   →  花括號 \\{C:fake\\} 與 \\@ not a placeholder",
            "Unknown key (verbatim)    →  未知參數: @{oops}",
            "Unknown style (verbatim)  →  未知樣式: {C:oops}hi{/C}",
        };

        private static MioTextSettings BuildSettings()
        {
            var settings = ScriptableObject.CreateInstance<MioTextSettings>();
            settings.Styles = new List<MioTextStyle>
            {
                new MioTextStyle { Name = "buff",   Color = new Color(0.35f, 0.90f, 0.40f), IsBold = true },
                new MioTextStyle { Name = "damage", Color = new Color(1.00f, 0.35f, 0.35f), IsBold = true },
                new MioTextStyle { Name = "heal",   Color = new Color(0.40f, 0.90f, 0.45f) },
                new MioTextStyle { Name = "focus",  Color = new Color(1.00f, 0.85f, 0.20f) },
                new MioTextStyle { Name = "frozen", Color = new Color(0.50f, 0.75f, 1.00f), IsItalic = true },
            };
            return settings;
        }

        private static void AddText(Transform parent, TMP_FontAsset font, string content, float y, float x, float width, TextAlignmentOptions align, bool gray)
        {
            var go = new GameObject("RowText");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(width, 32f);
            rt.anchoredPosition = new Vector2(x, y);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.fontSize = 18f;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;
            tmp.color = gray ? new Color(0.55f, 0.55f, 0.55f) : Color.white;
            tmp.text = content;
        }
    }
}
