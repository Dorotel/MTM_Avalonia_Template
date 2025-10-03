using System;
using System.Collections.Generic;

namespace MTM_Template_Application.Models.Navigation;

/// <summary>
/// Navigation history entry for tracking user navigation
/// </summary>
public class NavigationHistoryEntry
{
    /// <summary>
    /// Name of the view/page navigated to
    /// </summary>
    public string ViewName { get; set; } = string.Empty;

    /// <summary>
    /// When navigation occurred
    /// </summary>
    public DateTimeOffset NavigatedAtUtc { get; set; }

    /// <summary>
    /// Navigation parameters passed to the view
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Whether user can navigate back from this entry
    /// </summary>
    public bool CanGoBack { get; set; }

    /// <summary>
    /// Whether user can navigate forward from this entry
    /// </summary>
    public bool CanGoForward { get; set; }
}
