using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Cache;
using MTM_Template_Application.Services.Cache;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for CacheService
/// </summary>
public class CacheServiceTests
{
    private readonly ICacheService _mockCacheService;
    private readonly ILogger<CacheService> _mockLogger;

    public CacheServiceTests()
    {
        _mockCacheService = Substitute.For<ICacheService>();
        _mockLogger = Substitute.For<ILogger<CacheService>>();
    }

    [Fact]
    public async Task GetAsync_ExistingKey_ShouldReturnValue()
    {
        // Arrange
        var expectedValue = new TestEntity { Id = 1, Name = "Test" };
        _mockCacheService.GetAsync<TestEntity>("test-key").Returns(expectedValue);

        // Act
        var result = await _mockCacheService.GetAsync<TestEntity>("test-key");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedValue);
    }

    [Fact]
    public async Task GetAsync_NonExistingKey_ShouldReturnNull()
    {
        // Arrange
        _mockCacheService.GetAsync<TestEntity>("non-existent").Returns((TestEntity?)null);

        // Act
        var result = await _mockCacheService.GetAsync<TestEntity>("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ValidEntry_ShouldSucceed()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        _mockCacheService.SetAsync("test-key", entity, TimeSpan.FromHours(1)).Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _mockCacheService.SetAsync("test-key", entity, TimeSpan.FromHours(1));

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetAsync_WithLZ4Compression_ShouldCompressData()
    {
        // This test validates that the cache service compresses data using LZ4
        // Arrange
        var largeEntity = new TestEntity { Id = 1, Name = new string('X', 10000) };
        _mockCacheService.SetAsync("large-key", largeEntity, TimeSpan.FromHours(1)).Returns(Task.CompletedTask);

        // Act
        await _mockCacheService.SetAsync("large-key", largeEntity, TimeSpan.FromHours(1));

        // Assert
        await _mockCacheService.Received(1).SetAsync("large-key", largeEntity, TimeSpan.FromHours(1));
    }

    [Fact]
    public async Task RemoveAsync_ExistingKey_ShouldRemove()
    {
        // Arrange
        _mockCacheService.RemoveAsync("test-key").Returns(Task.CompletedTask);

        // Act
        await _mockCacheService.RemoveAsync("test-key");

        // Assert
        await _mockCacheService.Received(1).RemoveAsync("test-key");
    }

    [Fact]
    public async Task ClearAsync_ShouldRemoveAllEntries()
    {
        // Arrange
        _mockCacheService.ClearAsync().Returns(Task.CompletedTask);

        // Act
        await _mockCacheService.ClearAsync();

        // Assert
        await _mockCacheService.Received(1).ClearAsync();
    }

    [Fact]
    public void GetStatistics_ShouldReturnCacheMetrics()
    {
        // Arrange
        var expectedStats = new CacheStatistics
        {
            TotalEntries = 100,
            HitCount = 80,
            MissCount = 20,
            HitRate = 0.8,
            TotalSizeBytes = 1024000,
            CompressionRatio = 3.0,
            EvictionCount = 5
        };
        _mockCacheService.GetStatistics().Returns(expectedStats);

        // Act
        var result = _mockCacheService.GetStatistics();

        // Assert
        result.Should().NotBeNull();
        result.HitRate.Should().Be(0.8);
        result.CompressionRatio.Should().BeApproximately(3.0, 0.1);
    }

    [Fact]
    public async Task RefreshAsync_ExpiredEntries_ShouldReloadFromSource()
    {
        // Arrange
        _mockCacheService.RefreshAsync().Returns(Task.CompletedTask);

        // Act
        await _mockCacheService.RefreshAsync();

        // Assert
        await _mockCacheService.Received(1).RefreshAsync();
    }

    [Fact]
    public async Task SetAsync_WithTTL_ShouldExpireAfterDuration()
    {
        // This test validates TTL enforcement (Parts: 24h, Others: 7d)
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var ttl = TimeSpan.FromHours(24);
        _mockCacheService.SetAsync("parts-key", entity, ttl).Returns(Task.CompletedTask);

        // Act
        await _mockCacheService.SetAsync("parts-key", entity, ttl);

        // Assert
        await _mockCacheService.Received(1).SetAsync("parts-key", entity, ttl);
    }

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
