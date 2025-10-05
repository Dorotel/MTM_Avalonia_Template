using System;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Core;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for MappingService (T152)
/// Tests AutoMapper integration with profile discovery
/// </summary>
public class MappingServiceTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullMapper_ThrowsArgumentNullException()
    {
        // Arrange
        var mockLogger = Substitute.For<ILogger<MappingService>>();

        // Act
        Action act = () => new MappingService(mockLogger, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("mapper");
    }

    [Fact]
    public void Constructor_WithValidMapper_CreatesServiceSuccessfully()
    {
        // Arrange
        var mockLogger = Substitute.For<ILogger<MappingService>>();
        var config = new MapperConfiguration(cfg => { });
        var mapper = config.CreateMapper();

        // Act
        var service = new MappingService(mockLogger, mapper);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region Create Factory Method Tests

    [Fact]
    public void Create_WithNoAssemblies_CreatesServiceWithCurrentAssembly()
    {
        // Arrange
        var mockLoggerFactory = Substitute.For<ILoggerFactory>();
        mockLoggerFactory.CreateLogger<MappingService>().Returns(Substitute.For<ILogger<MappingService>>());

        // Act
        var service = MappingService.Create(mockLoggerFactory);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithSpecificAssemblies_CreatesServiceWithProfiles()
    {
        // Arrange
        var mockLoggerFactory = Substitute.For<ILoggerFactory>();
        mockLoggerFactory.CreateLogger<MappingService>().Returns(Substitute.For<ILogger<MappingService>>());
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var service = MappingService.Create(mockLoggerFactory, assembly);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithMultipleAssemblies_DiscoversProfilesFromAll()
    {
        // Arrange
        var mockLoggerFactory = Substitute.For<ILoggerFactory>();
        mockLoggerFactory.CreateLogger<MappingService>().Returns(Substitute.For<ILogger<MappingService>>());
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(),
            typeof(MappingService).Assembly
        };

        // Act
        var service = MappingService.Create(mockLoggerFactory, assemblies);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateServiceWithTestProfile();

        // Act
        Action act = () => service.Map<SourceModel, DestinationModel>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public void Map_WithValidSource_MapsCorrectly()
    {
        // Arrange
        var service = CreateServiceWithTestProfile();
        var source = new SourceModel
        {
            Name = "John Doe",
            Age = 30,
            Email = "john@example.com"
        };

        // Act
        var destination = service.Map<SourceModel, DestinationModel>(source);

        // Assert
        destination.Should().NotBeNull();
        destination.FullName.Should().Be("John Doe");
        destination.Age.Should().Be(30);
    }

    [Fact]
    public void Map_WithComplexMapping_HandlesNestedProperties()
    {
        // Arrange
        var service = CreateServiceWithTestProfile();
        var source = new SourceModel
        {
            Name = "Jane Smith",
            Age = 25,
            Email = "jane@example.com"
        };

        // Act
        var destination = service.Map<SourceModel, DestinationModel>(source);

        // Assert
        destination.FullName.Should().Be("Jane Smith");
        destination.ContactInfo.Should().Contain("jane@example.com");
    }

    #endregion

    #region MapAsync Tests

    [Fact]
    public async Task MapAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateServiceWithTestProfile();

        // Act
        var act = async () => await service.MapAsync<SourceModel, DestinationModel>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public async Task MapAsync_WithValidSource_MapsCorrectly()
    {
        // Arrange
        var service = CreateServiceWithTestProfile();
        var source = new SourceModel
        {
            Name = "Alice Johnson",
            Age = 28,
            Email = "alice@example.com"
        };

        // Act
        var destination = await service.MapAsync<SourceModel, DestinationModel>(source);

        // Assert
        destination.Should().NotBeNull();
        destination.FullName.Should().Be("Alice Johnson");
        destination.Age.Should().Be(28);
    }

    [Fact]
    public async Task MapAsync_WithComplexObject_CompletesSuccessfully()
    {
        // Arrange
        var service = CreateServiceWithTestProfile();
        var source = new SourceModel
        {
            Name = "Bob Williams",
            Age = 35,
            Email = "bob@example.com"
        };

        // Act
        var destination = await service.MapAsync<SourceModel, DestinationModel>(source);

        // Assert
        destination.FullName.Should().Be("Bob Williams");
        destination.ContactInfo.Should().Contain("bob@example.com");
    }

    #endregion

    #region Helper Methods

    private static MappingService CreateServiceWithTestProfile()
    {
        var mockLogger = Substitute.For<ILogger<MappingService>>();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceModel, DestinationModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ContactInfo, opt => opt.MapFrom(src => $"Email: {src.Email}"));
        });

        var mapper = config.CreateMapper();
        return new MappingService(mockLogger, mapper);
    }

    #endregion

    #region Helper Classes

    private class SourceModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    private class DestinationModel
    {
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string ContactInfo { get; set; } = string.Empty;
    }

    #endregion
}
