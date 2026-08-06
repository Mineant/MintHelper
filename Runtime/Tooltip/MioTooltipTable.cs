using System;
using System.Collections.Generic;
using UnityEngine;

namespace MioHelper.Tooltip
{
    /// <summary>
    /// Static keyword → content table. The built-in <see cref="IMioTooltipProvider"/> for
    /// projects whose tooltip data is authored as assets; set it on
    /// <see cref="MioTooltipManager.Table"/> or pass one to a manager. Rows are matched by
    /// keyword, case-insensitive. Can also be edited at runtime via <see cref="SetContent"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "MioTooltipTable", menuName = "MioHelper/Tooltip/Mio Tooltip Table")]
    public class MioTooltipTable : ScriptableObject, IMioTooltipProvider
    {
        [Tooltip("Keyword → content rows. First case-insensitive match wins.")]
        public List<MioTooltipContent> Entries = new List<MioTooltipContent>();

        public bool TryGetTooltip(string keyword, out MioTooltipContent content)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                for (int i = 0; i < Entries.Count; i++)
                {
                    MioTooltipContent entry = Entries[i];
                    if (entry != null && entry.Keyword != null
                        && entry.Keyword.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        content = entry;
                        return true;
                    }
                }
            }

            content = null;
            return false;
        }

        /// <summary>Insert or replace a row (matched by Keyword, case-insensitive).</summary>
        public void SetContent(MioTooltipContent content)
        {
            if (content == null || string.IsNullOrEmpty(content.Keyword)) return;

            for (int i = 0; i < Entries.Count; i++)
            {
                MioTooltipContent entry = Entries[i];
                if (entry != null && entry.Keyword != null
                    && entry.Keyword.Equals(content.Keyword, StringComparison.OrdinalIgnoreCase))
                {
                    Entries[i] = content;
                    return;
                }
            }

            Entries.Add(content);
        }

        /// <summary>Remove a row by keyword. Returns true if a row was removed.</summary>
        public bool RemoveContent(string keyword)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                MioTooltipContent entry = Entries[i];
                if (entry != null && entry.Keyword != null
                    && entry.Keyword.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    Entries.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void Clear() => Entries.Clear();
    }
}
