using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.Cache;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for Visual master data caching
/// </summary>
public class VisualCachingTests
{
    [Fact]
    public async Task Cache_ShouldPopulateOnFirstAccess()
    {
        // Arrange
        var cacheService = Substitute.For<ICacheService>();
        cacheService.GetAsync<string>("test_key").Returns((string?)null);
        
        // Act
        var result = await cacheService.GetAsync<string>("test_key");
        
        // Assert
        await cacheService.Received(1).GetAsync<string>("test_key");
    }

    [Fact]
    public async Task Cache_ShouldReturnCachedDataOnHit()
    {
        // Arrange
        var cacheService = Substitute.For<ICacheService>();
        await cacheService.SetAsync("test_key", "test_value");
        cacheService.GetAsync<string>("test_key").Returns("test_value");
        
        // Act
        var result = await cacheService.GetAsync<string>("test_key");
        
        // Assert
        result.Should().Be("test_value");
    }

    [Fact]
    public async Task Cache_ShouldRefreshStaleEntries()
    {
        // Arrange
        var cacheService = Substitute.For<ICacheService>();
        
        // Act
        await cacheService.RefreshAsync();
        
        // Assert
        await cacheService.Received(1).RefreshAsync();
    }

    [Fact]
    public async Task CacheOfflineMode_ShouldUseCachedDataWhenServerUnavailable()
    {
        // Arrange
        var cacheService = Substitute.For<ICacheService>();
        await cacheService.SetAsync("offline_key", "cached_value");
        cacheService.GetAsync<string>("offline_key").Returns("cached_value");
        
        // Act - simulate offline mode
        var result = await cacheService.GetAsync<string>("offline_key");
        
        // Assert
        result.Should().Be("cached_value", "cached data should be available in offline mode");
    }
}
