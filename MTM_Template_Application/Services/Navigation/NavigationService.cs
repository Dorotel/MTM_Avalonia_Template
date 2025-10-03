using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Navigation;

namespace MTM_Template_Application.Services.Navigation;

/// <summary>
/// Navigation service - Stack-based navigation, history tracking, deep linking
/// </summary>
public class NavigationService : INavigationService
{
    private readonly Stack<NavigationHistoryEntry> _backStack;
    private readonly Stack<NavigationHistoryEntry> _forwardStack;
    private readonly UnsavedChangesGuard _unsavedChangesGuard;
    private NavigationHistoryEntry? _currentEntry;

    public NavigationService(UnsavedChangesGuard unsavedChangesGuard)
    {
        ArgumentNullException.ThrowIfNull(unsavedChangesGuard);

        _backStack = new Stack<NavigationHistoryEntry>();
        _forwardStack = new Stack<NavigationHistoryEntry>();
        _unsavedChangesGuard = unsavedChangesGuard;
    }

    public async Task NavigateToAsync(string viewName, Dictionary<string, object>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        // Check for unsaved changes
        if (_currentEntry != null && !await _unsavedChangesGuard.CanNavigateAsync())
        {
            return; // Navigation cancelled by user
        }

        // Save current entry to back stack
        if (_currentEntry != null)
        {
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

        // TODO: Actual navigation logic would go here (e.g., update ViewModel, raise event)
        await Task.CompletedTask;
    }

    public async Task<bool> GoBackAsync()
    {
        if (_backStack.Count == 0)
        {
            return false;
        }

        // Check for unsaved changes
        if (!await _unsavedChangesGuard.CanNavigateAsync())
        {
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

        // TODO: Actual navigation logic
        return true;
    }

    public async Task<bool> GoForwardAsync()
    {
        if (_forwardStack.Count == 0)
        {
            return false;
        }

        // Check for unsaved changes
        if (!await _unsavedChangesGuard.CanNavigateAsync())
        {
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

        // Add forward stack (newest to oldest)
        history.AddRange(_forwardStack);

        return history;
    }
}
