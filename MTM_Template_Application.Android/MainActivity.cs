using System;
using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Android.Services;
using MTM_Template_Application.ViewModels;
using MTM_Template_Application.Views;
using Serilog;
using Serilog.Events;

namespace MTM_Template_Application.Android;

[Activity(
    Label = "MTM_Template_Application.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private static ServiceProvider? _serviceProvider;

    protected override void OnCreate(global::Android.OS.Bundle? savedInstanceState)
    {
        // Configure Serilog early for boot logging
        ConfigureSerilog();

        try
        {
            // Initialize DI container
            _serviceProvider = ConfigureServices();

            Log.Information("MainActivity initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to initialize MainActivity");
            throw;
        }

        base.OnCreate(savedInstanceState);
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
        // Don't show splash here - App.axaml.cs handles it for Android via ISingleViewApplicationLifetime
    }

    protected override void OnDestroy()
    {
        Log.CloseAndFlush();
        _serviceProvider?.Dispose();
        base.OnDestroy();
    }

    /// <summary>
    /// Configure Serilog for application logging.
    /// </summary>
    private static void ConfigureSerilog()
    {
        var logPath = System.IO.Path.Combine(
            global::Android.OS.Environment.ExternalStorageDirectory?.AbsolutePath ?? "/sdcard",
            "MTM_Template",
            "logs",
            "app-.txt"
        );

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "MTM_Template_Application")
            .Enrich.WithProperty("Platform", "Android")
            .WriteTo.Console()
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024 // 10MB
            )
            .CreateLogger();

        Log.Information("Android application starting...");
    }

    /// <summary>
    /// Configure dependency injection container.
    /// </summary>
    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Add Serilog.ILogger (required by LoggingService) - MUST BE FIRST
        services.AddSingleton(Log.Logger);
        Log.Debug("Registered Serilog.ILogger");

        // Add Microsoft.Extensions.Logging with Serilog
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });
        Log.Debug("Configured Microsoft.Extensions.Logging");

        // Build a temporary provider to get logger factory
        Log.Debug("Building temporary service provider for logger factory");
        var tempProvider = services.BuildServiceProvider();
        var loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();
        Log.Debug("Logger factory obtained");

        // Register all Android platform services
        Log.Information("Registering Android platform services...");
        services.AddAndroidServices(loggerFactory);
        Log.Information("Android services registered successfully");

        Log.Information("Building final service provider...");
        var provider = services.BuildServiceProvider();
        Log.Information("Service provider built successfully");

        // Dispose temp provider
        tempProvider.Dispose();

        Log.Information("Dependency injection container configured for Android");

        return provider;
    }

    // ShowSplashScreen removed - App.axaml.cs handles splash via ISingleViewApplicationLifetime

    /// <summary>
    /// Get a service from the DI container (for use by App.axaml.cs if needed).
    /// </summary>
    public static T? GetService<T>() where T : class
    {
        return _serviceProvider?.GetService<T>();
    }

    /// <summary>
    /// Get the service provider itself (for use by App.axaml.cs).
    /// </summary>
    public static IServiceProvider? GetServiceProvider()
    {
        return _serviceProvider;
    }
}
