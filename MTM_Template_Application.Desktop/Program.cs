using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Desktop.Services;
using MTM_Template_Application.ViewModels;
using MTM_Template_Application.Views;
using Serilog;
using Serilog.Events;

namespace MTM_Template_Application.Desktop;

sealed class Program
{
    private static ServiceProvider? _serviceProvider;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Set main thread name immediately
        System.Threading.Thread.CurrentThread.Name = "Main-UI";

        // Configure Serilog early for boot logging
        ConfigureSerilog();
        Log.Information("[Main] ========================================");
        Log.Information("[Main] MTM Template Application Starting");
        Log.Information("[Main] Platform: Desktop | .NET: {Framework}", Environment.Version);
        Log.Information("[Main] Arguments: {Args}", args.Length > 0 ? string.Join(", ", args) : "(none)");
        Log.Information("[Main] ========================================");

        try
        {
            Log.Information("[Main] Phase 1: Initializing Dependency Injection container");
            _serviceProvider = ConfigureServices();
            Log.Information("[Main] Phase 1 Complete: DI container initialized");

            Log.Information("[Main] Phase 2: Building and starting Avalonia application");
            Log.Verbose("[Main] About to call BuildAvaloniaApp()");
            var appBuilder = BuildAvaloniaApp();
            Log.Debug("[Main] AppBuilder created, calling StartWithClassicDesktopLifetime");
            Log.Verbose("[Main] Flushing logs before Avalonia startup");
            Log.CloseAndFlush();
            System.Threading.Thread.Sleep(100); // Give logs time to flush

            // Reconfigure logger for Avalonia lifetime
            ConfigureSerilog();
            Log.Information("[Main] Avalonia starting...");

            appBuilder.StartWithClassicDesktopLifetime(args);

            Log.Information("[Main] Application exited normally");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "[Main] *** FATAL ERROR *** Application terminated unexpectedly");
            Log.CloseAndFlush();
            System.Threading.Thread.Sleep(200); // Ensure log is written
            throw;
        }
        finally
        {
            Log.Information("[Main] Cleanup: Closing logger and disposing services");
            Log.CloseAndFlush();
            _serviceProvider?.Dispose();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        Log.Verbose("[BuildAvaloniaApp] Creating AppBuilder");
        var builder = AppBuilder.Configure<App>();
        Log.Verbose("[BuildAvaloniaApp] Configuring platform detection");
        builder.UsePlatformDetect();
        Log.Verbose("[BuildAvaloniaApp] Configuring Inter font");
        builder.WithInterFont();
        Log.Verbose("[BuildAvaloniaApp] Enabling trace logging");
        builder.LogToTrace();
        Log.Debug("[BuildAvaloniaApp] AppBuilder configured successfully");
        return builder;
    }

    /// <summary>
    /// Configure Serilog for application logging.
    /// </summary>
    private static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "MTM_Template_Application")
            .Enrich.WithProperty("Platform", "Desktop")
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("ProcessId", Environment.ProcessId)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/app-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

        Log.Information("[ConfigureSerilog] Serilog configured: Console + File (logs/app-.txt)");
        Log.Information("[ConfigureSerilog] Log levels: Debug (default), Information (Microsoft)");
        Log.Information("[ConfigureSerilog] Machine: {Machine}, Process: {ProcessId}", Environment.MachineName, Environment.ProcessId);
    }

    /// <summary>
    /// Configure dependency injection container.
    /// </summary>
    private static ServiceProvider ConfigureServices()
    {
        try
        {
            Log.Information("Configuring services...");
            var services = new ServiceCollection();

            // Add Serilog.ILogger (required by LoggingService)
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

            // Register all Desktop platform services
            Log.Information("Registering Desktop platform services...");
            services.AddDesktopServices(loggerFactory, includeVisualApi: true);
            Log.Information("Desktop services registered successfully");

            Log.Information("Building final service provider...");
            var provider = services.BuildServiceProvider();
            Log.Information("Service provider built successfully");

            // Dispose temp provider
            tempProvider.Dispose();

            Log.Information("Dependency injection container configured successfully");

            return provider;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to configure services");
            throw;
        }
    }

    /// <summary>
    /// Show splash screen and start boot sequence.
    /// </summary>
    private static void ShowSplashScreen()
    {
        if (_serviceProvider == null)
        {
            Log.Error("Service provider not initialized - cannot show splash screen");
            return;
        }

        try
        {
            Log.Information("Attempting to resolve SplashViewModel...");
            var splashViewModel = _serviceProvider.GetRequiredService<SplashViewModel>();
            Log.Information("SplashViewModel resolved successfully");

            Log.Information("Creating SplashWindow...");
            var splashWindow = new SplashWindow
            {
                DataContext = splashViewModel
            };
            Log.Information("SplashWindow created successfully");

            Log.Information("Showing splash window...");
            splashWindow.Show();

            Log.Information("Splash screen displayed successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to show splash screen");
            throw;
        }
    }

    /// <summary>
    /// Get a service from the DI container (for use by App.axaml.cs if needed).
    /// </summary>
    public static T? GetService<T>() where T : class
    {
        return _serviceProvider?.GetService<T>();
    }

    /// <summary>
    /// Get the service provider (for use by App.axaml.cs).
    /// </summary>
    public static IServiceProvider? GetServiceProvider()
    {
        return _serviceProvider;
    }
}
