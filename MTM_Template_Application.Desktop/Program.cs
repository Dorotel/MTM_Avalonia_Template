using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Extensions;
using MTM_Template_Application.Services.Secrets;
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
        // Configure Serilog early for boot logging
        ConfigureSerilog();

        try
        {
            // Initialize DI container
            _serviceProvider = ConfigureServices();

            // Start Avalonia application
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
            _serviceProvider?.Dispose();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .AfterSetup(builder =>
            {
                // Show splash screen after Avalonia is initialized
                if (_serviceProvider != null)
                {
                    ShowSplashScreen();
                }
            });

    /// <summary>
    /// Configure Serilog for application logging.
    /// </summary>
    private static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "MTM_Template_Application")
            .Enrich.WithProperty("Platform", "Desktop")
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/app-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024 // 10MB
            )
            .CreateLogger();

        Log.Information("Application starting...");
    }

    /// <summary>
    /// Configure dependency injection container.
    /// </summary>
    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        // Build a temporary provider to get logger factory
        var tempProvider = services.BuildServiceProvider();
        var loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();

        // Platform-specific secrets service (Windows)
        ISecretsService secretsService = OperatingSystem.IsWindows()
            ? new WindowsSecretsService(loggerFactory.CreateLogger<WindowsSecretsService>())
            : OperatingSystem.IsMacOS()
                ? new MacOSSecretsService(loggerFactory.CreateLogger<MacOSSecretsService>())
                : throw new PlatformNotSupportedException("Unsupported platform for desktop application");

        // Add all application services
        services.AddAllServices(secretsService, includeVisualApi: true);

        var provider = services.BuildServiceProvider();

        // Dispose temp provider
        tempProvider.Dispose();

        Log.Information("Dependency injection container configured");

        return provider;
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
            var splashViewModel = _serviceProvider.GetRequiredService<SplashViewModel>();
            var splashWindow = new SplashWindow
            {
                DataContext = splashViewModel
            };

            splashWindow.Show();

            Log.Information("Splash screen displayed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to show splash screen");
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
}
