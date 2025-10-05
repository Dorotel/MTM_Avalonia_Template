using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using MTM_Template_Application.ViewModels;
using Serilog;

namespace MTM_Template_Application.Views;

public partial class SplashView : UserControl
{
    public SplashView()
    {
        Log.Debug("[SplashView] Constructor - Initializing component");
        InitializeComponent();
        Log.Debug("[SplashView] InitializeComponent() completed");

        // Subscribe to boot completion
        this.Loaded += (sender, args) =>
        {
            Log.Information("[SplashView] View loaded, checking for ViewModel");

            if (DataContext is SplashViewModel viewModel)
            {
                Log.Information("[SplashView] ViewModel found, subscribing to PropertyChanged");

                viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(SplashViewModel.IsBootInProgress))
                    {
                        if (!viewModel.IsBootInProgress)
                        {
                            Log.Information("[SplashView] Boot sequence completed (IsBootInProgress=false)");

                            if (!viewModel.HasError)
                            {
                                Log.Information("[SplashView] Boot completed successfully, transitioning to MainView");
                                TransitionToMainView();
                            }
                            else
                            {
                                Log.Error("[SplashView] Boot completed with errors, not transitioning");
                            }
                        }
                    }
                    else if (e.PropertyName == nameof(SplashViewModel.HasError))
                    {
                        if (viewModel.HasError)
                        {
                            Log.Error("[SplashView] Boot sequence failed: {Error}", viewModel.ErrorMessage);
                        }
                    }
                };

                // Start boot sequence
                Log.Information("[SplashView] Starting boot sequence");
                _ = viewModel.StartBootSequenceAsync();
            }
            else
            {
                Log.Warning("[SplashView] DataContext is not SplashViewModel");
            }
        };
    }

    /// <summary>
    /// Transition from splash screen to main view on Android.
    /// </summary>
    private void TransitionToMainView()
    {
        try
        {
            Log.Debug("[SplashView] TransitionToMainView - Entry");

            if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                Log.Debug("[SplashView] Detected SingleViewApplicationLifetime (Android)");

                // Get service provider to resolve MainViewModel
                var serviceProvider = GetAndroidServiceProvider();

                if (serviceProvider != null)
                {
                    Log.Debug("[SplashView] Service provider obtained, resolving MainViewModel");
                    var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                    Log.Information("[SplashView] MainViewModel resolved successfully");

                    // Pass boot logs to MainViewModel
                    if (DataContext is SplashViewModel splashViewModel)
                    {
                        var bootLogs = splashViewModel.GetBootLogs();
                        var bootDuration = splashViewModel.GetBootDurationMs();
                        var bootMemory = splashViewModel.GetBootMemoryMB();

                        Log.Information("[SplashView] Transferring {LogCount} boot logs to MainViewModel", bootLogs.Count);
                        mainViewModel.SetBootLogs(bootLogs, (int)bootDuration, bootMemory);
                    }

                    Log.Debug("[SplashView] Creating MainView");
                    var mainView = new MainView
                    {
                        DataContext = mainViewModel
                    };
                    Log.Information("[SplashView] MainView created");

                    Log.Information("[SplashView] Setting MainView as primary view");
                    singleViewPlatform.MainView = mainView;
                    Log.Information("[SplashView] Transition to MainView completed successfully");
                }
                else
                {
                    Log.Error("[SplashView] Service provider is null, cannot create MainViewModel");
                }
            }
            else
            {
                Log.Warning("[SplashView] Not running in SingleViewApplicationLifetime mode");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[SplashView] Failed to transition to MainView");
        }
    }

    /// <summary>
    /// Get the service provider from MainActivity via reflection.
    /// </summary>
    private IServiceProvider? GetAndroidServiceProvider()
    {
        try
        {
            Log.Verbose("[SplashView] Looking up MainActivity type via reflection");
            var mainActivityType = Type.GetType("MTM_Template_Application.Android.MainActivity, MTM_Template_Application.Android");

            if (mainActivityType == null)
            {
                Log.Error("[SplashView] Could not find MainActivity type");
                return null;
            }

            Log.Verbose("[SplashView] Looking up GetServiceProvider method");
            var method = mainActivityType.GetMethod("GetServiceProvider", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            if (method == null)
            {
                Log.Error("[SplashView] Could not find GetServiceProvider method");
                return null;
            }

            Log.Verbose("[SplashView] Invoking GetServiceProvider");
            return method.Invoke(null, null) as IServiceProvider;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[SplashView] Exception getting Android service provider");
            return null;
        }
    }
}
