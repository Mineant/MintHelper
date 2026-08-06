Tooltip Example (click + hover tooltip module)
==============================================

A self-bootstrapping demo of MioHelper.Tooltip. No scene assets needed — attach
SampleTooltipDemo to any GameObject in a scene and press Play.

Setup
-----
1. Import this sample (Window > Package Manager > Mio Helper > Samples > Tooltip Example).
2. Open an empty scene.
3. Create an empty GameObject and add the "SampleTooltipDemo" component.
4. Press Play.

What you should see
-------------------
- Body text with two clickable links (查看劍 / 查看盾), styled with {C:focus}.
- A grey "hover me" panel in the middle.

What to try
-----------
- Click 查看劍: a tooltip opens near the click with a {C:rare} colored name, an
  @{durability} value formatted via {N:int}, and a nested link 盾牌.
- Click the nested 盾牌: the tooltip re-shows for the shield (defense 15), which
  in turn has a nested 藥水 link.
- Click 藥水: shows the potion (heal 50).
- Hover the grey panel: a hover tooltip shows with @{hp} resolved; it disappears
  when the pointer leaves the panel.
- Close any tooltip with the 關閉 button or by clicking empty space.

How it works
------------
- SampleTooltipDemo.BuildDemo() builds everything at runtime: an EventSystem
  (scene-less samples get none), an overlay canvas, a MioTextSettings asset, a
  MioTooltipTable asset, the body TMP text with {L:} links, the tooltip view
  (a TooltipUIProduct with a close button), and a MioTooltipManager.
- {L:keyword} is converted to a TMP <link="keyword"> by MioTextFormatter.
  MioTooltipLink (attached to the body text) catches the click and asks the
  manager to show the keyword's tooltip.
- Content comes from the MioTooltipTable. A project with dynamic content instead
  implements IMioTooltipProvider and calls MioTooltipManager.SetProvider(...).
- Hover uses MioTooltipHoverTrigger (IPointerEnter/Exit) with a static
  MioTooltipContent, or an IProvideTooltip on the same GameObject.
- Tooltip content is itself MioText template text, so {C:}/{N:}/@{key}/nested
  {L:} links all work inside a tooltip (nested links get a MioTooltipLink
  attached automatically by the manager).

Notes
-----
- The manager needs an EventSystem for pointer events.
- The click-outside blocker covers the tooltip's canvas — keep body links and
  the tooltip on one canvas.
- Unknown {L:oops} keywords log a warning in dev builds and are ignored
  (existing tooltip state is left unchanged).
