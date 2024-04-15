using System;
using System.Collections;
using System.Collections.Generic;
using Mineant;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Mineant
{
    public class TooltipManager : MMPersistentSingleton<TooltipManager>
    {
        // Check when the player is hover over a thing
        public TooltipUIProduct Tooltip;

        private bool _showingProduct;

        void Update()
        {
            if (ProcessUIRaycast()) { }
            else if (Process2DRaycast()) { }
        }

        private bool Process2DRaycast()
        {
            IProvideTooltip provideTooltip = null;
            RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            foreach (RaycastHit2D hit in hits)
            {
                provideTooltip = hit.collider.GetComponent<IProvideTooltip>();
                if (provideTooltip != null && !provideTooltip.CanProvideTooltip())
                {
                    provideTooltip = null;
                    continue;
                }
                if (provideTooltip != null) break;
            }

            if (provideTooltip == null)
            {
                HideTooltip();
                return false;
            }

            ShowTooltip(provideTooltip.GetTooltip());

            return true;
        }

        private bool ProcessUIRaycast()
        {
            List<RaycastResult> results = Helpers.GetEventSystemRaycastResults();
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.layer == LayerMask.NameToLayer("UI") && result.gameObject.TryGetComponent<IProvideTooltip>(out IProvideTooltip provideTooltip) && provideTooltip.CanProvideTooltip())
                {
                    ShowTooltip(provideTooltip.GetTooltip());
                    return true;
                }
            }

            HideTooltip();

            return false;
        }


        public void ShowTooltip(TooltipArgs args)
        {
            if (_showingProduct)
            {
                if (Tooltip.transform.position != args.Position) HideTooltip();
            }
            if (_showingProduct) return;

            _showingProduct = true;
            Tooltip.transform.position = args.Position;
            Tooltip.Generate(new TooltipProductArgs(args));
        }

        public void HideTooltip()
        {
            if (!_showingProduct) return;

            _showingProduct = false;
            Tooltip.Hide();
        }


    }

    public class TooltipArgs
    {
        public Vector3 Position;
        public string Name;
        public string Description;
        public Sprite Icon;

        public TooltipArgs(Vector3 position, Sprite icon, string name, string description)
        {
            Position = position;
            Icon = icon;
            Name = name;
            Description = description;
        }

    }
}
