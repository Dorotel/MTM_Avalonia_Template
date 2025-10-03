using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Navigation;

namespace MTM_Template_Application.Services.Navigation;

/// <summary>
/// Navigation service with history tracking
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigate to a view
    /// </summary>
    Task NavigateToAsync(string viewName, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// Navigate back
    /// </summary>
    Task<bool> GoBackAsync();

    /// <summary>
    /// Navigate forward
    /// </summary>
    Task<bool> GoForwardAsync();

    /// <summary>
    /// Get navigation history
    /// </summary>
    List<NavigationHistoryEntry> GetHistory();
}
