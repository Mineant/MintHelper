using UnityEngine;
using UnityEngine.EventSystems;

namespace MioHelper.Tooltip
{
    /// <summary>
    /// Hover trigger for UI objects. Shows a tooltip on pointer-enter and hides it on
    /// pointer-exit. Content comes from <see cref="Content"/> (static) or, failing that, an
    /// <see cref="IProvideTooltip"/> on the same GameObject. Place on the same object as (or a
    /// parent of) the Graphic that receives the pointer events.
    /// </summary>
    [AddComponentMenu("MioHelper/UI/Mio Tooltip Hover Trigger")]
    public class MioTooltipHoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("Optional static content shown on hover. If null, falls back to an IProvideTooltip on this GameObject.")]
        public MioTooltipContent Content;

        [Tooltip("Manager routed to on hover. Null → MioTooltipManager.Instance.")]
        public MioTooltipManager Manager;

        public void OnPointerEnter(PointerEventData eventData)
        {
            MioTooltipManager manager = Manager != null ? Manager : MioTooltipManager.Instance;
            if (manager == null) return;

            if (Content != null)
            {
                manager.ShowTooltip(Content, eventData.position);
                return;
            }

            if (TryGetComponent(out IProvideTooltip provider) && provider.CanProvideTooltip())
            {
                manager.ShowTooltip(provider.GetTooltip(), eventData.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MioTooltipManager manager = Manager != null ? Manager : MioTooltipManager.Instance;
            if (manager != null) manager.HideTooltip();
        }
    }
}
