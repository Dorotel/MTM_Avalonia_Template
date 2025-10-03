using System;
using System.Threading.Tasks;

namespace MTM_Template_Application.Services.Navigation;

/// <summary>
/// Guard to prompt user before navigation when unsaved changes exist
/// </summary>
public class UnsavedChangesGuard
{
    private Func<Task<bool>>? _hasUnsavedChangesCallback;
    private Func<Task<bool>>? _confirmNavigationCallback;

    /// <summary>
    /// Register callback to check for unsaved changes
    /// </summary>
    public void RegisterUnsavedChangesCheck(Func<Task<bool>> callback)
    {
        _hasUnsavedChangesCallback = callback;
    }

    /// <summary>
    /// Register callback to confirm navigation (e.g., show dialog)
    /// </summary>
    public void RegisterConfirmationDialog(Func<Task<bool>> callback)
    {
        _confirmNavigationCallback = callback;
    }

    /// <summary>
    /// Check if navigation can proceed
    /// </summary>
    public async Task<bool> CanNavigateAsync()
    {
        // No check registered - allow navigation
        if (_hasUnsavedChangesCallback == null)
        {
            return true;
        }

        // Check for unsaved changes
        var hasUnsavedChanges = await _hasUnsavedChangesCallback();
        
        if (!hasUnsavedChanges)
        {
            return true; // No unsaved changes - allow navigation
        }

        // Has unsaved changes - confirm with user
        if (_confirmNavigationCallback != null)
        {
            return await _confirmNavigationCallback();
        }

        // No confirmation dialog registered - block navigation by default
        return false;
    }

    /// <summary>
    /// Clear all registered callbacks
    /// </summary>
    public void Clear()
    {
        _hasUnsavedChangesCallback = null;
        _confirmNavigationCallback = null;
    }
}
