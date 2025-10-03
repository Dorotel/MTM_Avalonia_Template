using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MTM_Template_Application.ViewModels;

namespace MTM_Template_Application.Views;

/// <summary>
/// Splash window displayed during application boot.
/// Theme-less window with progress indication.
/// </summary>
public partial class SplashWindow : Window
{
    public SplashWindow()
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
    /// Called when the window is opened.
    /// Starts the boot sequence automatically.
    /// </summary>
    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (DataContext is SplashViewModel viewModel)
        {
            // Start boot sequence when window opens
            await viewModel.StartBootSequenceAsync();

            // Close splash window when boot completes
            if (!viewModel.IsBootInProgress)
            {
                // Wait a brief moment so user sees "Application ready!" message
                await System.Threading.Tasks.Task.Delay(500);
                Close();
            }
        }
    }

    /// <summary>
    /// Handle window closing.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is SplashViewModel viewModel)
        {
            viewModel.Dispose();
        }

        base.OnClosed(e);
    }
}
