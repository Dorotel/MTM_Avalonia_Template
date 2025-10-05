using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MTM_Template_Application.Models.Configuration;

namespace MTM_Template_Application.Views.Configuration;

/// <summary>
/// Modal dialog for displaying critical configuration errors
/// Blocks UI until user addresses the issue or dismisses
/// </summary>
public partial class ConfigurationErrorDialog : Window
{
    private bool _retryRequested;

    public ConfigurationErrorDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Show dialog with configuration error
    /// </summary>
    /// <param name="owner">Parent window (for centering)</param>
    /// <param name="error">Configuration error to display</param>
    /// <returns>True if user clicked Retry, false if closed/cancelled</returns>
    public static async Task<bool> ShowDialogAsync(Window? owner, ConfigurationError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(owner);

        var dialog = new ConfigurationErrorDialog
        {
            DataContext = error,
            Title = GetDialogTitle(error.Severity)
        };

        await dialog.ShowDialog(owner);

        return dialog._retryRequested;
    }

    /// <summary>
    /// Get appropriate dialog title based on error severity
    /// </summary>
    private static string GetDialogTitle(ErrorSeverity severity)
    {
        return severity switch
        {
            ErrorSeverity.Critical => "Critical Configuration Error",
            ErrorSeverity.Warning => "Configuration Warning",
            ErrorSeverity.Info => "Configuration Information",
            _ => "Configuration Error"
        };
    }

    /// <summary>
    /// Retry button clicked - user wants to retry the operation
    /// </summary>
    private void RetryButton_Click(object? sender, RoutedEventArgs e)
    {
        _retryRequested = true;
        Close();
    }

    /// <summary>
    /// Close button clicked - user dismisses the error
    /// </summary>
    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        _retryRequested = false;
        Close();
    }
}
