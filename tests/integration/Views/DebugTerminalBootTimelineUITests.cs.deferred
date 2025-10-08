using FluentAssertions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Boot;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Boot;
using MTM_Template_Application.Services.Diagnostics;
using MTM_Template_Application.Services.Diagnostics;
using MTM_Template_Application.ViewModels;
using MTM_Template_Application.ViewModels;
using NSubstitute;
using NSubstitute;
using Xunit;

namespace MTM_Template_Application.Tests.Integration.Views;

using Xunit;



/// <summary>namespace MTM_Template_Application.Tests.Integration.Views;

/// Integration tests for Debug Terminal Boot Timeline UI workflow.

/// Tests ViewModel-level UI logic and service integration for boot timeline visualization./// <summary>

/// Per T044: Verify boot timeline bar chart, color-coding (green/red/gray), and Stage 1 expansion./// Integration tests for Debug Terminal Boot Timeline UI workflow.

/// Note: Full UI rendering tests deferred - testing ViewModel contracts here./// Tests ViewModel-level UI logic and service integration for boot timeline visualization.

/// </summary>/// Per T044: Verify boot timeline bar chart, color-coding (green/red/gray), and Stage 1 expansion.

public class DebugTerminalBootTimelineUITests/// Note: Full UI rendering tests deferred - testing ViewModel contracts here.

{/// </summary>

    private readonly IPerformanceMonitoringService _mockPerformanceService; public class DebugTerminalBootTimelineUITests

    private readonly IDiagnosticsServiceExtensions _mockDiagnosticsService;{

    private readonly IExportService _mockExportService; private readonly IPerformanceMonitoringService _mockPerformanceService;

    private readonly ILogger<DebugTerminalViewModel> _mockLogger; private readonly IDiagnosticsServiceExtensions _mockDiagnosticsService;

    private readonly IExportService _mockExportService;

    public DebugTerminalBootTimelineUITests()    private readonly ILogger<DebugTerminalViewModel> _mockLogger;

    {

        _mockPerformanceService = Substitute.For<IPerformanceMonitoringService>();    public DebugTerminalBootTimelineUITests()

        _mockDiagnosticsService = Substitute.For<IDiagnosticsServiceExtensions>();    {

        _mockExportService = Substitute.For<IExportService>();        _mockPerformanceService = Substitute.For<IPerformanceMonitoringService>();

        _mockLogger = Substitute.For<ILogger<DebugTerminalViewModel>>();        _mockDiagnosticsService = Substitute.For<IDiagnosticsServiceExtensions>();

    }
_mockExportService = Substitute.For<IExportService>();

        _mockLogger = Substitute.For<ILogger<DebugTerminalViewModel>>();

    /// <summary>    }

    /// T044 Test Case 1: Verify boot timeline bar chart data is loaded and displayed.

    /// Tests RefreshBootTimelineCommand populates CurrentBootTimeline property.    /// <summary>

    /// </summary>    /// T044 Test Case 1: Verify boot timeline bar chart data is loaded and displayed.

    [Fact]    /// Tests RefreshBootTimelineCommand populates CurrentBootTimeline property.

public async Task BootTimeline_Should_Load_And_Display_Current_Timeline()    /// </summary>

{
    [Fact]

    // Arrange    public async Task BootTimeline_Should_Load_And_Display_Current_Timeline()

    var sampleTimeline = CreateSampleBootTimeline();
    {

        _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())        // Arrange

            .Returns(sampleTimeline); var sampleTimeline = CreateSampleBootTimeline();

        _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())

        var viewModel = new DebugTerminalViewModel(            .Returns(sampleTimeline);

        _mockLogger,

            performanceMonitoringService: _mockPerformanceService,        var viewModel = new DebugTerminalViewModel(

            diagnosticsServiceExtensions: _mockDiagnosticsService, _mockLogger,

            exportService: _mockExportService            performanceMonitoringService: _mockPerformanceService,

        ); diagnosticsServiceExtensions: _mockDiagnosticsService,

            exportService: _mockExportService

        // Act        );

        await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

        // Act

        // Assert        await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

        viewModel.CurrentBootTimeline.Should().NotBeNull("boot timeline should be loaded");

        viewModel.CurrentBootTimeline.Should().Be(sampleTimeline);        // Assert

        await _mockDiagnosticsService.Received(1).GetBootTimelineAsync(Arg.Any<CancellationToken>()); viewModel.CurrentBootTimeline.Should().NotBeNull("boot timeline should be loaded");

    }
    viewModel.CurrentBootTimeline.Should().Be(sampleTimeline);

    await _mockDiagnosticsService.Received(1).GetBootTimelineAsync(Arg.Any<CancellationToken>());

    /// <summary>    }

    /// T044 Test Case 2: Verify TotalBootTime calculation matches Stage 0/1/2 sum.

    /// Per data-model.md: TotalBootTime = Stage0.Duration + Stage1.Duration + Stage2.Duration.    /// <summary>

    /// </summary>    /// T044 Test Case 2: Verify TotalBootTime calculation matches Stage 0/1/2 sum.

    [Fact]    /// Per data-model.md: TotalBootTime = Stage0.Duration + Stage1.Duration + Stage2.Duration.

    public async Task TotalBootTime_Should_Equal_Sum_Of_Stage_Durations()    /// </summary>

    {
        [Fact]

        // Arrange    public async Task TotalBootTime_Should_Equal_Sum_Of_Stage_Durations()

        var timeline = new BootTimeline    {

        {        // Arrange

            BootStartTime = DateTime.UtcNow.AddMinutes(-5),        var timeline = new BootTimeline

            Stage0 = new Stage0Info        {

            {            BootStartTime = DateTime.UtcNow.AddMinutes(-5),

                Duration = TimeSpan.FromSeconds(1),            Stage0 = new Stage0Info

                Success = true,            {

                ErrorMessage = null                Duration = TimeSpan.FromMilliseconds(1000), // 1 second

            },                Success = true,

            Stage1 = new Stage1Info                ErrorMessage = null

            {            },

                Duration = TimeSpan.FromSeconds(3),            Stage1 = new Stage1Info

                Success = true,            {

                ErrorMessage = null,                Duration = TimeSpan.FromMilliseconds(3000), // 3 seconds

                ServiceTimings = new List<ServiceInitInfo>()                Success = true,

            },                ErrorMessage = null,

            Stage2 = new Stage2Info                ServiceTimings = new List<ServiceInitInfo>()

            {            },

                Duration = TimeSpan.FromMilliseconds(500),            Stage2 = new Stage2Info

                Success = true,            {

                ErrorMessage = null                Duration = TimeSpan.FromMilliseconds(500), // 0.5 seconds

            },                Success = true,

            TotalBootTime = TimeSpan.FromMilliseconds(4500)                ErrorMessage = null

        };            },

            TotalBootTime = TimeSpan.FromMilliseconds(4500)

        _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())        }
    ;

            .Returns(timeline);

    _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())

        var viewModel = new DebugTerminalViewModel(            .Returns(timeline);

    _mockLogger,

            performanceMonitoringService: _mockPerformanceService,        var viewModel = new DebugTerminalViewModel(

            diagnosticsServiceExtensions: _mockDiagnosticsService, _mockLogger,

            exportService: _mockExportService            performanceMonitoringService: _mockPerformanceService,

        ); diagnosticsServiceExtensions: _mockDiagnosticsService,

            exportService: _mockExportService

        // Act        );

        await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

    // Act

    // Assert        await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

    var expectedTotal = TimeSpan.FromMilliseconds(4500); // 4.5 seconds

    viewModel.TotalBootTime.Should().Be(expectedTotal, "TotalBootTime should equal sum of all stage durations");        // Assert

}
var expectedTotal = TimeSpan.FromMilliseconds(1000 + 3000 + 500); // 4.5 seconds

viewModel.TotalBootTime.Should().Be(expectedTotal, "TotalBootTime should equal sum of all stage durations");

/// <summary>    }

/// T044 Test Case 3: Verify SlowestStage calculation identifies stage with longest duration.

/// Per ViewModelContracts.md: SlowestStage should be "Stage 0", "Stage 1", or "Stage 2".    /// <summary>

/// </summary>    /// T044 Test Case 3: Verify SlowestStage calculation identifies stage with longest duration.

[Fact]    /// Per ViewModelContracts.md: SlowestStage should be "Stage 0", "Stage 1", or "Stage 2".

public async Task SlowestStage_Should_Identify_Stage_With_Longest_Duration()    /// </summary>

{
    [Fact]

    // Arrange - Stage 1 is slowest (3 seconds)    public async Task SlowestStage_Should_Identify_Stage_With_Longest_Duration()

    var timeline = new BootTimeline    {

        {        // Arrange - Stage 1 is slowest (3 seconds)

            BootStartTime = DateTime.UtcNow.AddMinutes(-5),        var timeline = new BootTimeline

            Stage0 = new Stage0Info        {

            {            BootStartTime = DateTime.UtcNow.AddMinutes(-5),

                Duration = TimeSpan.FromSeconds(1),            Stage0 = new Stage0Info

                Success = true,            {

                ErrorMessage = null                Duration = TimeSpan.FromMilliseconds(1000), // 1 second

            },                Success = true,

            Stage1 = new Stage1Info                ErrorMessage = null

            {            },

                Duration = TimeSpan.FromSeconds(3), // slowest            Stage1 = new Stage1Info

                Success = true,            {

                ErrorMessage = null,                Duration = TimeSpan.FromMilliseconds(3000), // 3 seconds (slowest)

                ServiceTimings = new List<ServiceInitInfo>()                Success = true,

            },                ErrorMessage = null,

            Stage2 = new Stage2Info                ServiceTimings = new List<ServiceInitInfo>()

            {            },

                Duration = TimeSpan.FromMilliseconds(500),            Stage2 = new Stage2Info

                Success = true,            {

                ErrorMessage = null                Duration = TimeSpan.FromMilliseconds(500), // 0.5 seconds

            },                Success = true,

            TotalBootTime = TimeSpan.FromMilliseconds(4500)                ErrorMessage = null

        };            },

        TotalBootTime = TimeSpan.FromMilliseconds(4500)

        _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())        };

            .Returns(timeline);

_mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())

        var viewModel = new DebugTerminalViewModel(            .Returns(timeline);

_mockLogger,

            performanceMonitoringService: _mockPerformanceService,        var viewModel = new DebugTerminalViewModel(

            diagnosticsServiceExtensions: _mockDiagnosticsService, _mockLogger,

            exportService: _mockExportService            performanceMonitoringService: _mockPerformanceService,

        ); diagnosticsServiceExtensions: _mockDiagnosticsService,

            exportService: _mockExportService

        // Act        );

        await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

        // Act

        // Assert        await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

        viewModel.SlowestStage.Should().Contain("Stage 1", "Stage 1 has the longest duration (3000ms)");

    }        // Assert

        viewModel.SlowestStage.Should().Contain("Stage 1", "Stage 1 has the longest duration (3000ms)");

/// <summary>    }

/// T044 Test Case 4: Verify bar chart color-coding for Stage 0 (target <1s).

/// Green: meets target (<1s), Red: exceeds target (>=1s), Gray: no data.    /// <summary>

/// </summary>    /// T044 Test Case 4: Verify bar chart color-coding for Stage 0 (target <1s).

[Theory]    /// Green: meets target (<1s), Red: exceeds target (>=1s), Gray: no data.

[InlineData(800, true)]   // <1s = green (meets target)    /// </summary>

[InlineData(1200, false)] // >=1s = red (exceeds target)    [Theory]

public async Task BootTimeline_Stage0_Color_Should_Match_Target(int durationMs, bool meetsTarget)    [InlineData(800, true)]   // <1s = green (meets target)

{
    [InlineData(1200, false)] // >=1s = red (exceeds target)

    // Arrange    public async Task BootTimeline_Stage0_Color_Should_Match_Target(long durationMs, bool meetsTarget)

    var timeline = new BootTimeline    {

        {        // Arrange

            BootStartTime = DateTime.UtcNow.AddMinutes(-5),        var timeline = new BootTimeline

            Stage0 = new Stage0Info        {

            {            BootStartTime = DateTime.UtcNow.AddMinutes(-5),

                Duration = TimeSpan.FromMilliseconds(durationMs),            Stage0 = new Stage0Info

                Success = true,            {

                ErrorMessage = null                DurationMs = durationMs,

            },                Success = true,

            Stage1 = new Stage1Info                ErrorMessage = null

            {            },

                Duration = TimeSpan.FromSeconds(2),            Stage1 = new Stage1Info

                Success = true,            {

                ErrorMessage = null,                DurationMs = 2000,

                ServiceTimings = new List<ServiceInitInfo>()                Success = true,

            },                ErrorMessage = null,

            Stage2 = new Stage2Info                ServiceTimings = new List<ServiceInitInfo>()

            {            },

                Duration = TimeSpan.FromMilliseconds(500),            Stage2 = new Stage2Info

                Success = true,            {

                ErrorMessage = null                DurationMs = 500,

            },                Success = true,

            TotalBootTime = TimeSpan.FromMilliseconds(durationMs + 2000 + 500)                ErrorMessage = null

        };            }

        }
;

_mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())

    .Returns(timeline); _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())

    .Returns(timeline);

var viewModel = new DebugTerminalViewModel(

    _mockLogger, var viewModel = new DebugTerminalViewModel(

    performanceMonitoringService: _mockPerformanceService, _mockLogger,

    diagnosticsServiceExtensions: _mockDiagnosticsService, performanceMonitoringService: _mockPerformanceService,

    exportService: _mockExportService            diagnosticsServiceExtensions: _mockDiagnosticsService,


); exportService: _mockExportService

        );

// Act

await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);        // Act

await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

// Assert - Color conversion logic will be tested in T047 Value Converter tests

viewModel.CurrentBootTimeline.Should().NotBeNull();        // Assert - Color conversion logic will be tested in T047 Value Converter tests

if (meetsTarget) viewModel.CurrentBootTimeline.Should().NotBeNull();

{
    if (meetsTarget)

        viewModel.CurrentBootTimeline!.Stage0.Duration.TotalMilliseconds.Should().BeLessThan(1000,        {

        "Stage 0 duration <1s should render as green"); viewModel.CurrentBootTimeline!.Stage0.DurationMs.Should().BeLessThan(1000,

        }
    "Stage 0 duration <1s should render as green");

        else        }

{        else

        viewModel.CurrentBootTimeline!.Stage0.Duration.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(1000,        {

        "Stage 0 duration >=1s should render as red"); viewModel.CurrentBootTimeline!.Stage0.DurationMs.Should().BeGreaterThanOrEqualTo(1000,

        }
    "Stage 0 duration >=1s should render as red");

}        }

    }

    /// <summary>

    /// T044 Test Case 5: Verify Stage 1 service initialization details are available for expansion.    /// <summary>

    /// Per FR-009: Stage 1 should be expandable to show service initialization timings.    /// T044 Test Case 5: Verify Stage 1 service initialization details are available for expansion.

    /// </summary>    /// Per FR-009: Stage 1 should be expandable to show service initialization timings.

    [Fact]    /// </summary>

public async Task Stage1_Should_Contain_Service_Initialization_Details()    [Fact]

{    public async Task Stage1_Should_Contain_Service_Initialization_Details()

        // Arrange    {

        var serviceTimings = new List<ServiceInitInfo>        // Arrange

        {        var serviceTimings = new List<ServiceInitInfo>

            new ServiceInitInfo        {

            {            new ServiceInitInfo

                ServiceName = "ConfigurationService",            {

                Duration = TimeSpan.FromMilliseconds(150),                ServiceName = "ConfigurationService",

                Success = true                DurationMs = 150,

            },                Success = true

            new ServiceInitInfo            },

            {            new ServiceInitInfo

                ServiceName = "DatabaseService",            {

                Duration = TimeSpan.FromMilliseconds(500),                ServiceName = "DatabaseService",

                Success = true                DurationMs = 500,

            },                Success = true

            new ServiceInitInfo            },

            {            new ServiceInitInfo

                ServiceName = "LoggingService",            {

    Duration = TimeSpan.FromMilliseconds(80),                ServiceName = "LoggingService",

                Success = true                DurationMs = 80,

            }
Success = true

        };            }

        };

var timeline = new BootTimeline

        {        var timeline = new BootTimeline

            BootStartTime = DateTime.UtcNow.AddMinutes(-5),        {

            Stage0 = new Stage0Info            BootStartTime = DateTime.UtcNow.AddMinutes(-5),

            {            Stage0 = new Stage0Info

                Duration = TimeSpan.FromMilliseconds(800),            {

                Success = true,                DurationMs = 800,

                ErrorMessage = null                Success = true,

            },                ErrorMessage = null

            Stage1 = new Stage1Info            },

            {            Stage1 = new Stage1Info

                Duration = TimeSpan.FromMilliseconds(2500),            {

    Success = true,                DurationMs = 2500,

                ErrorMessage = null,                Success = true,

                ServiceTimings = serviceTimings                ErrorMessage = null,

            },                ServiceTimings = serviceTimings

            Stage2 = new Stage2Info            },

            {
    Stage2 = new Stage2Info

                Duration = TimeSpan.FromMilliseconds(500),            {

        Success = true,                DurationMs = 500,

                ErrorMessage = null                Success = true,

            },                ErrorMessage = null

            TotalBootTime = TimeSpan.FromMilliseconds(3800)            }

        };        };



_mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())        _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())

    .Returns(timeline);            .Returns(timeline);



var viewModel = new DebugTerminalViewModel(var viewModel = new DebugTerminalViewModel(

    _mockLogger, _mockLogger,

    performanceMonitoringService: _mockPerformanceService, performanceMonitoringService: _mockPerformanceService,

    diagnosticsServiceExtensions: _mockDiagnosticsService, diagnosticsServiceExtensions: _mockDiagnosticsService,

    exportService: _mockExportService            exportService: _mockExportService

);        );



// Act        // Act

await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null); await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);



// Assert        // Assert

viewModel.CurrentBootTimeline.Should().NotBeNull(); viewModel.CurrentBootTimeline.Should().NotBeNull();

viewModel.CurrentBootTimeline!.Stage1.ServiceTimings.Should().HaveCount(3, viewModel.CurrentBootTimeline!.Stage1.ServiceTimings.Should().HaveCount(3,

    "Stage 1 should contain 3 service initialization entries"); "Stage 1 should contain 3 service initialization entries");



viewModel.CurrentBootTimeline!.Stage1.ServiceTimings.Should().Contain(viewModel.CurrentBootTimeline!.Stage1.ServiceTimings.Should().Contain(

    s => s.ServiceName == "ConfigurationService" && s.Duration == TimeSpan.FromMilliseconds(150)            s => s.ServiceName == "ConfigurationService" && s.DurationMs == 150

);        );

viewModel.CurrentBootTimeline!.Stage1.ServiceTimings.Should().Contain(viewModel.CurrentBootTimeline!.Stage1.ServiceTimings.Should().Contain(

    s => s.ServiceName == "DatabaseService" && s.Duration == TimeSpan.FromMilliseconds(500)            s => s.ServiceName == "DatabaseService" && s.DurationMs == 500

);        );

    }    }



    /// <summary>    /// <summary>

    /// T044 Test Case 6: Verify bar widths are proportional to total boot time.    /// T044 Test Case 6: Verify bar widths are proportional to total boot time.

    /// Per FR-008: Bar widths should be proportional to stage durations relative to TotalBootTime.    /// Per FR-008: Bar widths should be proportional to stage durations relative to TotalBootTime.

    /// Note: Actual width calculation is done in XAML binding - this tests the data contract.    /// Note: Actual width calculation is done in XAML binding - this tests the data contract.

    /// </summary>    /// </summary>

    [Fact][Fact]

public async Task BarWidths_Should_Be_Proportional_To_Total_Boot_Time()    public async Task BarWidths_Should_Be_Proportional_To_Total_Boot_Time()

{
    {

        // Arrange        // Arrange

        var timeline = new BootTimeline        var timeline = new BootTimeline

        {        {

            BootStartTime = DateTime.UtcNow.AddMinutes(-5),            BootStartTime = DateTime.UtcNow.AddMinutes(-5),

            Stage0 = new Stage0Info            Stage0 = new Stage0Info

            {            {

                Duration = TimeSpan.FromSeconds(1), // 20% of total                DurationMs = 1000, // 20% of total (1s / 5s)

                Success = true,                Success = true,

                ErrorMessage = null                ErrorMessage = null

            },            },

            Stage1 = new Stage1Info            Stage1 = new Stage1Info

            {            {

                Duration = TimeSpan.FromSeconds(3), // 60% of total                DurationMs = 3000, // 60% of total (3s / 5s)

                Success = true,                Success = true,

                ErrorMessage = null,                ErrorMessage = null,

                ServiceTimings = new List<ServiceInitInfo>()                ServiceTimings = new List<ServiceInitInfo>()

            },            },

            Stage2 = new Stage2Info            Stage2 = new Stage2Info

            {            {

                Duration = TimeSpan.FromSeconds(1), // 20% of total                DurationMs = 1000, // 20% of total (1s / 5s)

                Success = true,                Success = true,

                ErrorMessage = null                ErrorMessage = null

            },            }

            TotalBootTime = TimeSpan.FromSeconds(5)        };

        };

        _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())

        _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>()).Returns(timeline);

            .Returns(timeline);

        var viewModel = new DebugTerminalViewModel(

        var viewModel = new DebugTerminalViewModel(_mockLogger,

            _mockLogger, performanceMonitoringService: _mockPerformanceService,

            performanceMonitoringService: _mockPerformanceService, diagnosticsServiceExtensions: _mockDiagnosticsService,

            diagnosticsServiceExtensions: _mockDiagnosticsService, exportService: _mockExportService

            exportService: _mockExportService);

        );

        // Act

        // Act        await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

        await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

        // Assert - Verify proportions (actual width calculation in XAML)

        // Assert - Verify proportions (actual width calculation in XAML)        var totalMs = 1000 + 3000 + 1000;

        var totalMs = timeline.Stage0.Duration.TotalMilliseconds + var stage0Proportion = (double)timeline.Stage0.DurationMs / totalMs;

        timeline.Stage1.Duration.TotalMilliseconds + var stage1Proportion = (double)timeline.Stage1.DurationMs / totalMs;

        timeline.Stage2.Duration.TotalMilliseconds; var stage2Proportion = (double)timeline.Stage2.DurationMs / totalMs;



        var stage0Proportion = timeline.Stage0.Duration.TotalMilliseconds / totalMs; stage0Proportion.Should().BeApproximately(0.20, 0.01, "Stage 0 should be ~20% of total");

        var stage1Proportion = timeline.Stage1.Duration.TotalMilliseconds / totalMs; stage1Proportion.Should().BeApproximately(0.60, 0.01, "Stage 1 should be ~60% of total");

        var stage2Proportion = timeline.Stage2.Duration.TotalMilliseconds / totalMs; stage2Proportion.Should().BeApproximately(0.20, 0.01, "Stage 2 should be ~20% of total");

    }

    stage0Proportion.Should().BeApproximately(0.20, 0.01, "Stage 0 should be ~20% of total");

    stage1Proportion.Should().BeApproximately(0.60, 0.01, "Stage 1 should be ~60% of total");    /// <summary>

    stage2Proportion.Should().BeApproximately(0.20, 0.01, "Stage 2 should be ~20% of total");    /// T044 Test Case 7: Verify boot timeline handles missing or null data gracefully.

}    /// Per FR-009: System should handle missing boot data without crashing (gray color).

     /// </summary>

     /// <summary>    [Fact]

     /// T044 Test Case 7: Verify boot timeline handles missing or null data gracefully.    public async Task BootTimeline_Should_Handle_Null_Data_Gracefully()

     /// Per FR-009: System should handle missing boot data without crashing (gray color).    {

     /// </summary>        // Arrange

[Fact] _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())

    public async Task BootTimeline_Should_Handle_Null_Data_Gracefully()            .Returns((BootTimeline?)null);

{

    // Arrange        var viewModel = new DebugTerminalViewModel(

    _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())            _mockLogger,

            .Returns((BootTimeline?)null); performanceMonitoringService: _mockPerformanceService,

            diagnosticsServiceExtensions: _mockDiagnosticsService,

        var viewModel = new DebugTerminalViewModel(exportService: _mockExportService

            _mockLogger,        );

performanceMonitoringService: _mockPerformanceService,

            diagnosticsServiceExtensions: _mockDiagnosticsService,        // Act

            exportService: _mockExportService await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

        );

    // Assert

    // Act        viewModel.CurrentBootTimeline.Should().BeNull("service returned null timeline");

    await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null); viewModel.TotalBootTime.Should().Be(TimeSpan.Zero, "TotalBootTime should be zero when no data");

    viewModel.SlowestStage.Should().BeNullOrEmpty("SlowestStage should be empty when no data");

    // Assert    }

    viewModel.CurrentBootTimeline.Should().BeNull("service returned null timeline");

    viewModel.TotalBootTime.Should().Be(TimeSpan.Zero, "TotalBootTime should be zero when no data");    /// <summary>

    viewModel.SlowestStage.Should().BeNullOrEmpty("SlowestStage should be empty when no data");    /// T044 Test Case 8: Verify refresh command calls diagnostics service and updates UI.

}    /// Per T031: RefreshBootTimelineCommand should load current boot timeline.

     /// </summary>

     /// <summary>    [Fact]

     /// T044 Test Case 8: Verify refresh command calls diagnostics service and updates UI.    public async Task RefreshButton_Should_Call_Service_And_Update_UI()

     /// Per T031: RefreshBootTimelineCommand should load current boot timeline.    {

     /// </summary>        // Arrange

[Fact] var timeline1 = CreateSampleBootTimeline(stage1DurationMs: 2000);

public async Task RefreshButton_Should_Call_Service_And_Update_UI()        var timeline2 = CreateSampleBootTimeline(stage1DurationMs: 3000);

{

    // Arrange        _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())

    var timeline1 = CreateSampleBootTimeline(stage1Duration: TimeSpan.FromSeconds(2));            .Returns(timeline1, timeline2);

    var timeline2 = CreateSampleBootTimeline(stage1Duration: TimeSpan.FromSeconds(3));

    var viewModel = new DebugTerminalViewModel(

    _mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())            _mockLogger,


        .Returns(timeline1, timeline2); performanceMonitoringService: _mockPerformanceService,

            diagnosticsServiceExtensions: _mockDiagnosticsService,

        var viewModel = new DebugTerminalViewModel(exportService: _mockExportService

            _mockLogger,        );

performanceMonitoringService: _mockPerformanceService,

            diagnosticsServiceExtensions: _mockDiagnosticsService,        // Act - First refresh

            exportService: _mockExportService await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

        );

    // Assert - First call

    // Act - First refresh        viewModel.CurrentBootTimeline.Should().Be(timeline1);

    await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

    // Act - Second refresh

    // Assert - First call        await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null);

    viewModel.CurrentBootTimeline.Should().Be(timeline1);

    // Assert - Second call

    // Act - Second refresh        viewModel.CurrentBootTimeline.Should().Be(timeline2);

    await viewModel.RefreshBootTimelineCommand.ExecuteAsync(null); await _mockDiagnosticsService.Received(2).GetBootTimelineAsync(Arg.Any<CancellationToken>());

}

// Assert - Second call

viewModel.CurrentBootTimeline.Should().Be(timeline2);    // Helper Methods

await _mockDiagnosticsService.Received(2).GetBootTimelineAsync(Arg.Any<CancellationToken>());

    }    /// <summary>

    /// Creates a sample boot timeline for testing.

    // Helper Methods    /// </summary>

    private static BootTimeline CreateSampleBootTimeline(

    /// <summary>        long stage0DurationMs = 900,

    /// Creates a sample boot timeline for testing.        long stage1DurationMs = 2500,

    /// </summary>        long stage2DurationMs = 600)

    private static BootTimeline CreateSampleBootTimeline(    {

        TimeSpan? stage0Duration = null,        return new BootTimeline

        TimeSpan? stage1Duration = null,        {

        TimeSpan? stage2Duration = null)            BootStartTime = DateTime.UtcNow.AddMinutes(-5),

    {            Stage0 = new Stage0Info

        var s0 = stage0Duration ?? TimeSpan.FromMilliseconds(900);
{

    var s1 = stage1Duration ?? TimeSpan.FromMilliseconds(2500); DurationMs = stage0DurationMs,

        var s2 = stage2Duration ?? TimeSpan.FromMilliseconds(600); Success = true,

                ErrorMessage = null

        return new BootTimeline            },

        {
    Stage1 = new Stage1Info

            BootStartTime = DateTime.UtcNow.AddMinutes(-5),            {

        Stage0 = new Stage0Info                DurationMs = stage1DurationMs,

            {
            Success = true,

                Duration = s0,                ErrorMessage = null,

                Success = true,                ServiceTimings = new List<ServiceInitInfo>

                ErrorMessage = null                {

            },                    new ServiceInitInfo

            Stage1 = new Stage1Info                    {

            {                        ServiceName = "ConfigurationService",

                Duration = s1,                        DurationMs = 150,

                Success = true,                        Success = true

                ErrorMessage = null,                    },

                ServiceTimings = new List<ServiceInitInfo>                    new ServiceInitInfo

                {                    {

                    new ServiceInitInfo                        ServiceName = "DatabaseService",

                    {                        DurationMs = 500,

                        ServiceName = "ConfigurationService",                        Success = true

                        Duration = TimeSpan.FromMilliseconds(150),                    }

                        Success = true                }

                    },            },

                    new ServiceInitInfo            Stage2 = new Stage2Info

                    {            {

                        ServiceName = "DatabaseService",                DurationMs = stage2DurationMs,

                        Duration = TimeSpan.FromMilliseconds(500),                Success = true,

                        Success = true                ErrorMessage = null

                    }            }

                }
}
;

            },    }

            Stage2 = new Stage2Info}

            {
    Duration = s2,
                Success = true,
                ErrorMessage = null
            },
            TotalBootTime = s0 + s1 + s2
        };
    }
}
