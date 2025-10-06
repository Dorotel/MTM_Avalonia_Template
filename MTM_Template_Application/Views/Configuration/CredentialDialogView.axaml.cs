using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MTM_Template_Application.ViewModels.Configuration;

namespace MTM_Template_Application.Views.Configuration;

/// <summary>
/// Code-behind for credential recovery dialog
/// Displays modal dialog for re-entering credentials when OS-native storage fails
/// </summary>
public partial class CredentialDialogView : Window
{
    public CredentialDialogView()
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
    /// Show dialog as modal and return result
    /// </summary>
    /// <param name="owner">Parent window (for centering)</param>
    /// <param name="viewModel">ViewModel instance with credential data</param>
    /// <returns>True if credentials were saved successfully, false if cancelled</returns>
    public static async Task<bool> ShowDialogAsync(Window? owner, CredentialDialogViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        var dialog = new CredentialDialogView
        {
            DataContext = viewModel
        };

        // Show as modal dialog
        await dialog.ShowDialog(owner ?? throw new ArgumentNullException(nameof(owner)));

        // Return dialog result
        return viewModel.GetDialogResult();
    }

    /// <summary>
    /// Show dialog with default ViewModel (for design-time or quick usage)
    /// </summary>
    /// <param name="owner">Parent window</param>
    /// <returns>True if credentials were saved, false if cancelled</returns>
    public async Task<bool> ShowDialogAsync(Window owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        if (DataContext is not CredentialDialogViewModel viewModel)
        {
            throw new InvalidOperationException(
                "DataContext must be set to CredentialDialogViewModel before showing dialog");
        }

        await ShowDialog(owner);
        return viewModel.GetDialogResult();
    }

    /// <summary>
    /// Close dialog with result
    /// Called by ViewModel after successful credential storage
    /// </summary>
    public void CloseWithResult(bool success)
    {
        // Dialog result is tracked in ViewModel
        // Just close the window
        Close();
    }
}
