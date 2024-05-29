
using UnityEngine;

namespace Mineant
{
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