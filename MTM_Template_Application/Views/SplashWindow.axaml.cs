using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using MTM_Template_Application.ViewModels;
using Serilog;

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
            Log.Information("[SplashWindow] Window opened, starting boot sequence");

            // Start boot sequence when window opens
            await viewModel.StartBootSequenceAsync();

            Log.Information("[SplashWindow] Boot sequence completed, transitioning to MainWindow");

            // Close splash window and show main window when boot completes
            if (!viewModel.HasError)
            {
                // Wait a brief moment so user sees "Application ready!" message
                await System.Threading.Tasks.Task.Delay(800);

                TransitionToMainWindow();
            }
            else
            {
                Log.Error("[SplashWindow] Boot sequence failed: {Error}", viewModel.ErrorMessage);
                // Keep splash window open to show error
            }
        }
    }

    /// <summary>
    /// Transition from splash screen to main application window.
    /// </summary>
    private void TransitionToMainWindow()
    {
        try
        {
            Log.Debug("[SplashWindow] Getting application lifetime");

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Log.Debug("[SplashWindow] Retrieving service provider for MainViewModel");

                // Get service provider from Program.cs
                var serviceProvider = GetServiceProvider();

                if (serviceProvider != null)
                {
                    Log.Debug("[SplashWindow] Resolving MainViewModel from DI");
                    var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();

                    // Pass boot logs to MainViewModel
                    if (DataContext is SplashViewModel splashViewModel)
                    {
                        var bootLogs = splashViewModel.GetBootLogs();
                        var bootDuration = splashViewModel.GetBootDurationMs();
                        var bootMemory = splashViewModel.GetBootMemoryMB();

                        Log.Information("[SplashWindow] Transferring {LogCount} boot logs to MainViewModel", bootLogs.Count);
                        mainViewModel.SetBootLogs(bootLogs, (int)bootDuration, bootMemory);
                    }

                    Log.Debug("[SplashWindow] Creating MainWindow");
                    var mainWindow = new MainWindow
                    {
                        DataContext = mainViewModel
                    };

                    Log.Information("[SplashWindow] Setting MainWindow as primary window");
                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();

                    Log.Information("[SplashWindow] Closing splash window");
                    Close();

                    Log.Information("[SplashWindow] Transition to MainWindow completed successfully");
                }
                else
                {
                    Log.Error("[SplashWindow] Service provider is null, cannot create MainViewModel");
                    // Fallback: create MainWindow without DI
                    var mainWindow = new MainWindow
                    {
                        DataContext = new MainViewModel()
                    };
                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();
                    Close();
                }
            }
            else
            {
                Log.Warning("[SplashWindow] Not running in ClassicDesktopStyleApplicationLifetime mode");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[SplashWindow] Failed to transition to MainWindow");
            // Keep splash window open
        }
    }

    /// <summary>
    /// Get service provider from Program.cs via reflection.
    /// </summary>
    private IServiceProvider? GetServiceProvider()
    {
        try
        {
            var programType = Type.GetType("MTM_Template_Application.Desktop.Program, MTM_Template_Application.Desktop");
            if (programType == null)
            {
                Log.Warning("[SplashWindow] Could not find Program type");
                return null;
            }

            var method = programType.GetMethod("GetServiceProvider", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method == null)
            {
                Log.Warning("[SplashWindow] Could not find GetServiceProvider method");
                return null;
            }

            return method.Invoke(null, null) as IServiceProvider;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[SplashWindow] Error getting service provider via reflection");
            return null;
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
