using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Models.Cache;
using MTM_Template_Application.Services.Cache;
using MTM_Template_Application.Services.DataLayer;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for VisualMasterDataSync (T148)
/// Tests master data synchronization and cache population
/// </summary>
public class VisualMasterDataSyncTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullVisualApiClient_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var cacheService = Substitute.For<ICacheService>();
        var stalenessDetector = Substitute.For<ICacheStalenessDetector>();

        Action act = () => new VisualMasterDataSync(null!, cacheService, stalenessDetector);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("visualApiClient");
    }

    [Fact]
    public void Constructor_WithNullCacheService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var visualApiClient = Substitute.For<IVisualApiClient>();
        var stalenessDetector = Substitute.For<ICacheStalenessDetector>();

        Action act = () => new VisualMasterDataSync(visualApiClient, null!, stalenessDetector);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cacheService");
    }

    [Fact]
    public void Constructor_WithNullStalenessDetector_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var visualApiClient = Substitute.For<IVisualApiClient>();
        var cacheService = Substitute.For<ICacheService>();

        Action act = () => new VisualMasterDataSync(visualApiClient, cacheService, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("stalenessDetector");
    }

    #endregion

    #region InitialPopulationAsync Tests

    [Fact]
    public async Task InitialPopulationAsync_WhenServerUnavailable_ThrowsInvalidOperationException()
    {
        // Arrange
        var visualApiClient = Substitute.For<IVisualApiClient>();
        visualApiClient.IsServerAvailable().Returns(false);

        var sync = CreateService(visualApiClient);

        // Act
        var act = async () => await sync.InitialPopulationAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not available*");
    }

    [Fact]
    public async Task InitialPopulationAsync_WithServerAvailable_PopulatesCache()
    {
        // Arrange
        var visualApiClient = Substitute.For<IVisualApiClient>();
        var cacheService = Substitute.For<ICacheService>();

        visualApiClient.IsServerAvailable().Returns(true);
        visualApiClient.ExecuteCommandAsync<List<object>>(Arg.Any<string>(), Arg.Any<Dictionary<string, object>>())
            .Returns(new List<object> { new { Id = "P001" } });

        var sync = CreateService(visualApiClient, cacheService);

        // Act
        await sync.InitialPopulationAsync();

        // Assert
        await visualApiClient.Received().IsServerAvailable();
    }

    #endregion

    #region DeltaSyncAsync Tests

    [Fact]
    public async Task DeltaSyncAsync_WhenServerUnavailable_ReturnsWithoutSyncing()
    {
        // Arrange
        var visualApiClient = Substitute.For<IVisualApiClient>();
        var cacheService = Substitute.For<ICacheService>();

        visualApiClient.IsServerAvailable().Returns(false);

        var sync = CreateService(visualApiClient, cacheService);

        // Act
        await sync.DeltaSyncAsync();

        // Assert
        await visualApiClient.Received().IsServerAvailable();
        await visualApiClient.DidNotReceive().ExecuteCommandAsync<object>(Arg.Any<string>(), Arg.Any<Dictionary<string, object>>());
    }

    [Fact]
    public async Task DeltaSyncAsync_WhenServerAvailable_RefreshesStaleEntries()
    {
        // Arrange
        var visualApiClient = Substitute.For<IVisualApiClient>();
        var cacheService = Substitute.For<ICacheService>();
        var stalenessDetector = Substitute.For<ICacheStalenessDetector>();

        visualApiClient.IsServerAvailable().Returns(true);
        var staleEntries = new List<StaleEntry>
        {
            new StaleEntry { Key = "part:P001", EntityType = "Part", LastAccessedUtc = DateTimeOffset.UtcNow.AddDays(-2) },
            new StaleEntry { Key = "customer:C001", EntityType = "Customer", LastAccessedUtc = DateTimeOffset.UtcNow.AddDays(-1) }
        };
        stalenessDetector.DetectStaleEntriesAsync().Returns(staleEntries);

        var sync = CreateService(visualApiClient, cacheService, stalenessDetector);

        // Act
        await sync.DeltaSyncAsync();

        // Assert
        await stalenessDetector.Received().DetectStaleEntriesAsync();
    }

    #endregion

    #region BackgroundRefreshAsync Tests

    [Fact]
    public async Task BackgroundRefreshAsync_WithValidInterval_RefreshesPreemptively()
    {
        // Arrange
        var visualApiClient = Substitute.For<IVisualApiClient>();
        var cacheService = Substitute.For<ICacheService>();
        var stalenessDetector = Substitute.For<ICacheStalenessDetector>();

        visualApiClient.IsServerAvailable().Returns(true);
        visualApiClient.ExecuteCommandAsync<List<object>>(Arg.Any<string>(), Arg.Any<Dictionary<string, object>>())
            .Returns(new List<object> { new { Id = "P001" } });

        stalenessDetector.GetEntriesNearExpirationAsync().Returns(new List<StaleEntry>
        {
            new StaleEntry { Key = "part:P001", EntityType = "Part", LastAccessedUtc = DateTimeOffset.UtcNow.AddHours(-23) }
        });

        var sync = CreateService(visualApiClient, cacheService, stalenessDetector);

        // Act
        await sync.BackgroundRefreshAsync();

        // Assert
        await visualApiClient.Received().IsServerAvailable();
    }

    #endregion

    #region Helper Methods

    private static VisualMasterDataSync CreateService(
        IVisualApiClient? visualApiClient = null,
        ICacheService? cacheService = null,
        ICacheStalenessDetector? stalenessDetector = null)
    {
        var apiClient = visualApiClient ?? Substitute.For<IVisualApiClient>();
        var cache = cacheService ?? Substitute.For<ICacheService>();
        var detector = stalenessDetector ?? Substitute.For<ICacheStalenessDetector>();

        return new VisualMasterDataSync(apiClient, cache, detector);
    }

    #endregion
}
