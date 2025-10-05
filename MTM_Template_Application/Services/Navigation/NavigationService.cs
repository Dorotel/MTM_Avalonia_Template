using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Navigation;

namespace MTM_Template_Application.Services.Navigation;

/// <summary>
/// Navigation service - Stack-based navigation, history tracking, deep linking
/// </summary>
public class NavigationService : INavigationService
{
    private readonly ILogger<NavigationService> _logger;
    private readonly Stack<NavigationHistoryEntry> _backStack;
    private readonly Stack<NavigationHistoryEntry> _forwardStack;
    private readonly UnsavedChangesGuard _unsavedChangesGuard;
    private NavigationHistoryEntry? _currentEntry;

    public NavigationService(
        ILogger<NavigationService> logger,
        UnsavedChangesGuard unsavedChangesGuard)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(unsavedChangesGuard);

        _logger = logger;

        _backStack = new Stack<NavigationHistoryEntry>();
        _forwardStack = new Stack<NavigationHistoryEntry>();
        _unsavedChangesGuard = unsavedChangesGuard;
    }

    public async Task NavigateToAsync(string viewName, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(viewName);

        _logger.LogInformation("Navigating to view: {ViewName}", viewName);

        // Check for unsaved changes
        if (_currentEntry != null && !await _unsavedChangesGuard.CanNavigateAsync())
        {
            _logger.LogWarning("Navigation to {ViewName} cancelled due to unsaved changes", viewName);
            return; // Navigation cancelled by user
        }

        // Save current entry to back stack
        if (_currentEntry != null)
        {
            // Update CanGoForward to false since we're moving forward to a new view
            _currentEntry.CanGoForward = false;
            _backStack.Push(_currentEntry);
        }

        // Clear forward stack on new navigation
        _forwardStack.Clear();

        // Create new entry
        _currentEntry = new NavigationHistoryEntry
        {
            ViewName = viewName,
            NavigatedAtUtc = DateTimeOffset.UtcNow,
            Parameters = parameters ?? new Dictionary<string, object>(),
            CanGoBack = _backStack.Count > 0,
            CanGoForward = false
        };

        _logger.LogDebug("Navigation complete. Back stack: {BackCount}, Forward stack: {ForwardCount}",
            _backStack.Count, _forwardStack.Count);

        // TODO: Actual navigation logic would go here (e.g., update ViewModel, raise event)
        await Task.CompletedTask;
    }

    public async Task<bool> GoBackAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_backStack.Count == 0)
        {
            _logger.LogDebug("Cannot go back - back stack is empty");
            return false;
        }

        _logger.LogInformation("Navigating back");

        // Check for unsaved changes
        if (!await _unsavedChangesGuard.CanNavigateAsync())
        {
            _logger.LogWarning("Back navigation cancelled due to unsaved changes");
            return false;
        }

        // Move current to forward stack
        if (_currentEntry != null)
        {
            _forwardStack.Push(_currentEntry);
        }

        // Pop from back stack
        _currentEntry = _backStack.Pop();
        _currentEntry.CanGoBack = _backStack.Count > 0;
        _currentEntry.CanGoForward = true;

        _logger.LogDebug("Back navigation complete to view: {ViewName}", _currentEntry.ViewName);

        // TODO: Actual navigation logic
        return true;
    }

    public async Task<bool> GoForwardAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_forwardStack.Count == 0)
        {
            _logger.LogDebug("Cannot go forward - forward stack is empty");
            return false;
        }

        _logger.LogInformation("Navigating forward");

        // Check for unsaved changes
        if (!await _unsavedChangesGuard.CanNavigateAsync())
        {
            _logger.LogWarning("Forward navigation cancelled due to unsaved changes");
            return false;
        }

        // Move current to back stack
        if (_currentEntry != null)
        {
            _backStack.Push(_currentEntry);
        }

        // Pop from forward stack
        _currentEntry = _forwardStack.Pop();
        _currentEntry.CanGoBack = true;
        _currentEntry.CanGoForward = _forwardStack.Count > 0;

        _logger.LogDebug("Forward navigation complete to view: {ViewName}", _currentEntry.ViewName);

        // TODO: Actual navigation logic
        return true;
    }

    public List<NavigationHistoryEntry> GetHistory()
    {
        var history = new List<NavigationHistoryEntry>();

        // Add back stack (oldest to newest)
        history.AddRange(_backStack.Reverse());

        // Add current entry
        if (_currentEntry != null)
        {
            history.Add(_currentEntry);
        }

        // Note: We don't include forward stack in history - it represents future navigation, not history

        return history;
    }
}
