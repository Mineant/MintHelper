namespace MioHelper.Tooltip
{
    /// <summary>
    /// Resolves a {L:keyword} TMP link id to raw tooltip content. Implementations may be
    /// data-driven (e.g. <see cref="MioTooltipTable"/>) or computed at runtime (a project's own
    /// config system, a dictionary, a remote table...).
    /// </summary>
    public interface IMioTooltipProvider
    {
        bool TryGetTooltip(string keyword, out MioTooltipContent content);
    }
}
