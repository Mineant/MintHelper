using System;
using System.Collections.Generic;

namespace MioHelper.StatSystem
{
    /// <summary>
    /// Inspector/authoring DTO: a whole table of modifier groups. Author it in the inspector,
    /// then Apply it onto a runtime <see cref="MioStatSheet"/> (optionally tracking a source so
    /// the same table can later be Removed by source).
    /// </summary>
    [Serializable]
    public class MioStatModifierTable
    {
        public List<MioStatModifierGroup> ModifierLists;

        public MioStatModifierTable()
        {
            ModifierLists = new List<MioStatModifierGroup>();
        }
    }
}
