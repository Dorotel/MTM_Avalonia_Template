# Developer Quickstart: Debug Terminal Modernization

**Time to First Feature**: ~5 minutes | **Date**: 2025-10-06

## Overview

This guide gets you implementing Debug Terminal features using **Test-Driven Development (TDD)**. Follow the pattern: Model → Service → ViewModel → XAML → Integration.

---

## Step 1: Create the Model (1 minute)

**Location**: `MTM_Template_Application/Models/`

```csharp
// PerformanceSnapshot.cs
namespace MTM_Template_Application.Models;

public sealed record PerformanceSnapshot
{
    public required DateTime Timestamp { get; init; }
    public required double CpuUsagePercent { get; init; }
    public required long MemoryUsageMB { get; init; }
    public required int GcGen0Collections { get; init; }
    public required int GcGen1Collections { get; init; }
    public required int GcGen2Collections { get; init; }
    public required int ThreadCount { get; init; }
    public required TimeSpan Uptime { get; init; }
}
```

**Test First**:
```csharp
// tests/unit/Models/PerformanceSnapshotTests.cs
public class PerformanceSnapshotTests
{
    [Fact]
    public void Should_Create_Valid_Snapshot()
    {
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = 25.5,
            MemoryUsageMB = 512,
            GcGen0Collections = 10,
            GcGen1Collections = 5,
            GcGen2Collections = 2,
            ThreadCount = 20,
            Uptime = TimeSpan.FromMinutes(5)
        };

        snapshot.CpuUsagePercent.Should().Be(25.5);
        snapshot.MemoryUsageMB.Should().Be(512);
    }
}
```

---

## Step 2: Create the Service Interface (1 minute)

**Location**: `specs/003-debug-terminal-modernization/contracts/`

```csharp
// IPerformanceMonitoringService.cs
namespace MTM_Template_Application.Services;

/// <summary>
/// Real-time system performance monitoring service.
/// </summary>
public interface IPerformanceMonitoringService
{
    /// <summary>
    /// Gets the current performance snapshot.
    /// </summary>
    Task<PerformanceSnapshot> GetCurrentSnapshotAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the last N performance snapshots.
    /// </summary>
    Task<IReadOnlyList<PerformanceSnapshot>> GetRecentSnapshotsAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts continuous performance monitoring.
    /// </summary>
    Task StartMonitoringAsync(TimeSpan interval, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops continuous performance monitoring.
    /// </summary>
    Task StopMonitoringAsync();
}
```

---

## Step 3: Implement the Service (2 minutes)

**Location**: `MTM_Template_Application/Services/`

```csharp
// PerformanceMonitoringService.cs
using System.Diagnostics;

namespace MTM_Template_Application.Services;

public sealed class PerformanceMonitoringService : IPerformanceMonitoringService
{
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly CircularBuffer<PerformanceSnapshot> _snapshots = new(capacity: 100);
    private PeriodicTimer? _timer;
    private Task? _monitoringTask;
    private readonly Process _currentProcess = Process.GetCurrentProcess();

    public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<PerformanceSnapshot> GetCurrentSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = GetCpuUsage(),
            MemoryUsageMB = _currentProcess.WorkingSet64 / (1024 * 1024),
            GcGen0Collections = GC.CollectionCount(0),
            GcGen1Collections = GC.CollectionCount(1),
            GcGen2Collections = GC.CollectionCount(2),
            ThreadCount = _currentProcess.Threads.Count,
            Uptime = DateTime.UtcNow - _currentProcess.StartTime
        };

        return Task.FromResult(snapshot);
    }

    public Task<IReadOnlyList<PerformanceSnapshot>> GetRecentSnapshotsAsync(int count, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_snapshots.TakeLast(count));
    }

    public async Task StartMonitoringAsync(TimeSpan interval, CancellationToken cancellationToken = default)
    {
        _timer = new PeriodicTimer(interval);
        _monitoringTask = Task.Run(async () =>
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                var snapshot = await GetCurrentSnapshotAsync(cancellationToken);
                _snapshots.Add(snapshot);
                _logger.LogDebug("Performance snapshot: CPU={Cpu}%, Memory={Memory}MB",
                    snapshot.CpuUsagePercent, snapshot.MemoryUsageMB);
            }
        }, cancellationToken);
    }

    public Task StopMonitoringAsync()
    {
        _timer?.Dispose();
        return _monitoringTask ?? Task.CompletedTask;
    }

    private double GetCpuUsage()
    {
        // Simplified - use PerformanceCounter or System.Diagnostics for accurate CPU%
        return Math.Round(Random.Shared.NextDouble() * 100, 2);
    }
}
```

**Test First**:
```csharp
// tests/unit/Services/PerformanceMonitoringServiceTests.cs
public class PerformanceMonitoringServiceTests
{
    private readonly ILogger<PerformanceMonitoringService> _logger;

    public PerformanceMonitoringServiceTests()
    {
        _logger = Substitute.For<ILogger<PerformanceMonitoringService>>();
    }

    [Fact]
    public async Task GetCurrentSnapshotAsync_Should_Return_Valid_Snapshot()
    {
        var service = new PerformanceMonitoringService(_logger);

        var snapshot = await service.GetCurrentSnapshotAsync();

        snapshot.Should().NotBeNull();
        snapshot.CpuUsagePercent.Should().BeInRange(0, 100);
        snapshot.MemoryUsageMB.Should().BeGreaterThan(0);
    }
}
```

---

## Step 4: Extend the ViewModel (1 minute)

**Location**: `MTM_Template_Application/ViewModels/DebugTerminalViewModel.cs`

```csharp
public partial class DebugTerminalViewModel : ObservableObject
{
    private readonly IPerformanceMonitoringService _performanceService;

    [ObservableProperty]
    private PerformanceSnapshot? _currentPerformance;

    [ObservableProperty]
    private bool _isMonitoring;

    public DebugTerminalViewModel(IPerformanceMonitoringService performanceService)
    {
        _performanceService = performanceService ?? throw new ArgumentNullException(nameof(performanceService));
    }

    [RelayCommand]
    private async Task StartMonitoringAsync(CancellationToken cancellationToken)
    {
        IsMonitoring = true;
        await _performanceService.StartMonitoringAsync(TimeSpan.FromSeconds(5), cancellationToken);
    }

    [RelayCommand]
    private async Task RefreshPerformanceAsync(CancellationToken cancellationToken)
    {
        CurrentPerformance = await _performanceService.GetCurrentSnapshotAsync(cancellationToken);
    }
}
```

**Test First**:
```csharp
// tests/unit/ViewModels/DebugTerminalViewModelTests.cs
public class DebugTerminalViewModelTests
{
    [Fact]
    public async Task RefreshPerformanceCommand_Should_Update_CurrentPerformance()
    {
        var mockService = Substitute.For<IPerformanceMonitoringService>();
        mockService.GetCurrentSnapshotAsync(Arg.Any<CancellationToken>())
            .Returns(new PerformanceSnapshot { /* properties */ });

        var viewModel = new DebugTerminalViewModel(mockService);

        await viewModel.RefreshPerformanceCommand.ExecuteAsync(null);

        viewModel.CurrentPerformance.Should().NotBeNull();
    }
}
```

---

## Step 5: Update the XAML (1 minute)

**Location**: `MTM_Template_Application/Views/DebugTerminalWindow.axaml`

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:MTM_Template_Application.ViewModels"
        x:Class="MTM_Template_Application.Views.DebugTerminalWindow"
        x:DataType="vm:DebugTerminalViewModel"
        x:CompileBindings="True"
        Title="Debug Terminal" Width="800" Height="600">

    <StackPanel Margin="20">
        <!-- Real-Time Performance Panel -->
        <Border BorderBrush="{DynamicResource ThemeBorderMidBrush}"
                BorderThickness="1"
                Padding="10"
                Margin="0,0,0,10">
            <StackPanel>
                <TextBlock Text="Real-Time Performance"
                           FontWeight="Bold"
                           FontSize="16"
                           Margin="0,0,0,10"/>

                <Grid ColumnDefinitions="Auto,*" RowDefinitions="Auto,Auto,Auto,Auto">
                    <TextBlock Grid.Row="0" Grid.Column="0" Text="CPU Usage:" Margin="0,0,10,5"/>
                    <TextBlock Grid.Row="0" Grid.Column="1"
                               Text="{CompiledBinding CurrentPerformance.CpuUsagePercent, StringFormat={}{0:F2}%}"/>

                    <TextBlock Grid.Row="1" Grid.Column="0" Text="Memory Usage:" Margin="0,0,10,5"/>
                    <TextBlock Grid.Row="1" Grid.Column="1"
                               Text="{CompiledBinding CurrentPerformance.MemoryUsageMB, StringFormat={}{0} MB}"/>

                    <TextBlock Grid.Row="2" Grid.Column="0" Text="Thread Count:" Margin="0,0,10,5"/>
                    <TextBlock Grid.Row="2" Grid.Column="1"
                               Text="{CompiledBinding CurrentPerformance.ThreadCount}"/>
                </Grid>

                <Button Content="Refresh"
                        Command="{CompiledBinding RefreshPerformanceCommand}"
                        Margin="0,10,0,0"/>
            </StackPanel>
        </Border>
    </StackPanel>
</Window>
```

---

## Step 6: Register Service in DI (30 seconds)

**Location**: `MTM_Template_Application.Desktop/Program.cs`

```csharp
public static AppBuilder BuildAvaloniaApp()
{
    return AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace()
        .ConfigureServices(services =>
        {
            // Register Performance Monitoring
            services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();
            services.AddTransient<DebugTerminalViewModel>();
        });
}
```

---

## Testing Strategy

### Unit Tests (80% coverage target)
- Models: Property validation, immutability
- Services: Business logic, error handling
- ViewModels: Command execution, property updates

### Integration Tests (Critical paths)
- Service → ViewModel integration
- Data flow end-to-end
- Performance validation (<2% CPU)

---

## Troubleshooting

### Issue: CompiledBinding errors
**Solution**: Ensure `x:DataType` is set on root Window/UserControl and matches ViewModel type.

### Issue: Service not injected
**Solution**: Verify service registered in `Program.cs` and constructor has correct parameter type.

### Issue: Performance monitoring high CPU
**Solution**: Increase refresh interval (default 5s per CL-002).

---

## Next Steps

1. ✅ Follow this pattern for each of the 20 features
2. ✅ Run tests after each step (TDD approach)
3. ✅ Validate constitutional compliance (CompiledBindings, Null Safety, DI)
4. ✅ Check performance after integration (<2% CPU, <500ms render)

**Reference**: See [data-model.md](./data-model.md) for all 5 entities and [contracts/](./contracts/) for service interfaces.

---

**Status**: ✅ **Complete**
**Document Version**: 1.0 | **Last Updated**: 2025-10-06
