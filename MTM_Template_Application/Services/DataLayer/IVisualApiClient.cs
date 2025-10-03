using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTM_Template_Application.Services.DataLayer;

/// <summary>
/// Visual ERP API client (read-only via API Toolkit)
/// </summary>
public interface IVisualApiClient
{
    /// <summary>
    /// Execute a whitelisted Visual API command
    /// </summary>
    Task<T?> ExecuteCommandAsync<T>(string command, Dictionary<string, object> parameters);

    /// <summary>
    /// Check if Visual server is available
    /// </summary>
    Task<bool> IsServerAvailable();

    /// <summary>
    /// Get list of whitelisted API commands
    /// </summary>
    List<string> GetWhitelistedCommands();
}
