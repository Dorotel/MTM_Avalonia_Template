using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using MTM_Template_Application.ViewModels;
using MTM_Template_Application.Views;
using Serilog;

namespace MTM_Template_Application;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        Log.Debug("[App] Initialize() - Starting Avalonia XAML load");
        AvaloniaXamlLoader.Load(this);
        Log.Information("[App] Initialize() - Avalonia XAML loaded successfully");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Log.Information("[App] OnFrameworkInitializationCompleted() - Framework initialization complete");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Log.Debug("[App] Detected ClassicDesktopStyleApplicationLifetime");

            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            Log.Verbose("[App] Disabling Avalonia data annotation validation");
            DisableAvaloniaDataAnnotationValidation();
            Log.Debug("[App] Avalonia validation plugins disabled");

            try
            {
                // Get service provider from Program.cs
                Log.Debug("[App] Attempting to retrieve service provider via reflection");
                _serviceProvider = GetServiceProvider();

                if (_serviceProvider != null)
                {
                    Log.Information("[App] Service provider obtained successfully");

                    Log.Debug("[App] Resolving SplashViewModel from DI container");
                    var splashViewModel = _serviceProvider.GetRequiredService<SplashViewModel>();
                    Log.Information("[App] SplashViewModel resolved successfully");

                    Log.Debug("[App] Creating SplashWindow instance");
                    var splashWindow = new SplashWindow
                    {
                        DataContext = splashViewModel
                    };
                    Log.Debug("[App] SplashWindow instance created");

                    Log.Debug("[App] Setting SplashWindow as MainWindow");
                    desktop.MainWindow = splashWindow;
                    Log.Information("[App] SplashWindow set as MainWindow successfully");
                }
                else
                {
                    Log.Warning("[App] Service provider is null - falling back to MainWindow");
                    // Fallback to MainWindow if no DI container
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = new MainViewModel()
                    };
                    Log.Information("[App] Fallback MainWindow created");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[App] CRITICAL ERROR in OnFrameworkInitializationCompleted");
                throw;
            }
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            Log.Debug("[App] Detected SingleViewApplicationLifetime (Android/iOS)");

            try
            {
                // Get service provider from MainActivity
                Log.Debug("[App] Attempting to retrieve service provider for Android");
                _serviceProvider = GetAndroidServiceProvider();

                if (_serviceProvider != null)
                {
                    Log.Information("[App] Android service provider obtained successfully");

                    Log.Debug("[App] Resolving SplashViewModel from DI container");
                    var splashViewModel = _serviceProvider.GetRequiredService<SplashViewModel>();
                    Log.Information("[App] SplashViewModel resolved successfully");

                    Log.Debug("[App] Creating SplashView instance");
                    var splashView = new SplashView
                    {
                        DataContext = splashViewModel
                    };
                    Log.Debug("[App] SplashView instance created");

                    Log.Debug("[App] Setting SplashView as MainView");
                    singleViewPlatform.MainView = splashView;
                    Log.Information("[App] SplashView set as MainView successfully");
                }
                else
                {
                    Log.Warning("[App] Service provider is null - falling back to MainView");
                    singleViewPlatform.MainView = new MainView
                    {
                        DataContext = new MainViewModel()
                    };
                    Log.Information("[App] Fallback MainView created");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[App] CRITICAL ERROR setting up Android MainView");
                throw;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        Log.Verbose("[App] DisableAvaloniaDataAnnotationValidation() - Entry");

        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        Log.Debug("[App] Found {PluginCount} DataAnnotationsValidationPlugin(s) to remove", dataValidationPluginsToRemove.Length);

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
            Log.Verbose("[App] Removed DataAnnotationsValidationPlugin: {PluginType}", plugin.GetType().Name);
        }

        Log.Verbose("[App] DisableAvaloniaDataAnnotationValidation() - Exit");
    }

    /// <summary>
    /// Get service provider from the desktop entry point.
    /// This allows the App to access DI services.
    /// </summary>
    private IServiceProvider? GetServiceProvider()
    {
        Log.Verbose("[App] GetServiceProvider() - Entry");

        try
        {
            // Use reflection to get the static method from Program class
            Log.Verbose("[App] Looking up Program type via reflection");
            var programType = Type.GetType("MTM_Template_Application.Desktop.Program, MTM_Template_Application.Desktop");

            if (programType == null)
            {
                Log.Error("[App] Could not find Program type via reflection");
                return null;
            }

            Log.Verbose("[App] Program type found: {TypeName}", programType.FullName);
            Log.Verbose("[App] Looking up GetServiceProvider static method");

            var method = programType.GetMethod("GetServiceProvider", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            if (method == null)
            {
                Log.Error("[App] Could not find GetServiceProvider method on Program type");
                return null;
            }

            Log.Verbose("[App] Invoking GetServiceProvider method");
            var result = method.Invoke(null, null) as IServiceProvider;

            if (result == null)
            {
                Log.Warning("[App] GetServiceProvider returned null");
            }
            else
            {
                Log.Debug("[App] Service provider retrieved successfully via reflection");
            }

            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[App] Exception in GetServiceProvider reflection call");
            return null;
        }
        finally
        {
            Log.Verbose("[App] GetServiceProvider() - Exit");
        }
    }

    /// <summary>
    /// Get service provider from Android MainActivity.
    /// This allows the App to access DI services on Android.
    /// </summary>
    private IServiceProvider? GetAndroidServiceProvider()
    {
        Log.Verbose("[App] GetAndroidServiceProvider() - Entry");

        try
        {
            // Use reflection to get the static method from MainActivity class
            Log.Verbose("[App] Looking up MainActivity type via reflection");
            var mainActivityType = Type.GetType("MTM_Template_Application.Android.MainActivity, MTM_Template_Application.Android");

            if (mainActivityType == null)
            {
                Log.Error("[App] Could not find MainActivity type via reflection");
                return null;
            }

            Log.Verbose("[App] MainActivity type found: {TypeName}", mainActivityType.FullName);
            Log.Verbose("[App] Looking up GetServiceProvider method");

            var method = mainActivityType.GetMethod("GetServiceProvider", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            if (method == null)
            {
                Log.Error("[App] Could not find GetServiceProvider method on MainActivity type");
                return null;
            }

            Log.Verbose("[App] Invoking GetServiceProvider method");
            var result = method.Invoke(null, null) as IServiceProvider;

            if (result == null)
            {
                Log.Warning("[App] GetServiceProvider returned null");
            }
            else
            {
                Log.Debug("[App] Android service provider retrieved successfully via reflection");
            }

            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[App] Exception in GetAndroidServiceProvider reflection call");
            return null;
        }
        finally
        {
            Log.Verbose("[App] GetAndroidServiceProvider() - Exit");
        }
    }
}
