using System.Collections.Generic;
using System.Threading;
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
    Task NavigateToAsync(string viewName, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Navigate back
    /// </summary>
    Task<bool> GoBackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Navigate forward
    /// </summary>
    Task<bool> GoForwardAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get navigation history
    /// </summary>
    List<NavigationHistoryEntry> GetHistory();
}
