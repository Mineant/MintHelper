using System;
using System.Collections.Generic;

namespace MioHelper.StatSystem
{
    /// <summary>Inspector/authoring DTO: a named group of <see cref="MioStatModifier"/>s.</summary>
    [Serializable]
    public class MioStatModifierGroup
    {
        public string Group;
        public List<MioStatModifier> Modifiers;

        public MioStatModifierGroup()
        {
            Modifiers = new List<MioStatModifier>();
        }
    }
}
