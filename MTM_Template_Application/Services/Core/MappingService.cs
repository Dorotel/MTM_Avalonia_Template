using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace MTM_Template_Application.Services.Core;

/// <summary>
/// Mapping service with AutoMapper integration and profile discovery
/// </summary>
public class MappingService : IMappingService
{
    private readonly ILogger<MappingService> _logger;
    private readonly IMapper _mapper;

    public MappingService(
        ILogger<MappingService> logger,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mapper);

        _logger = logger;
        _mapper = mapper;

        _logger.LogInformation("MappingService initialized with AutoMapper");
    }

    /// <summary>
    /// Create MappingService with auto-discovered profiles
    /// </summary>
    public static MappingService Create(ILoggerFactory loggerFactory, params Assembly[] assemblies)
    {
        var logger = loggerFactory.CreateLogger<MappingService>();
        logger.LogInformation("Creating MappingService with auto-discovered profiles from {AssemblyCount} assemblies",
            assemblies.Length > 0 ? assemblies.Length : 1);

        var config = new MapperConfiguration(cfg =>
        {
            // Auto-discover profiles from assemblies
            if (assemblies.Length > 0)
            {
                logger.LogDebug("Scanning assemblies: {Assemblies}",
                    string.Join(", ", assemblies.Select(a => a.GetName().Name)));
                cfg.AddMaps(assemblies);
            }
            else
            {
                // Default: scan current assembly
                logger.LogDebug("Scanning executing assembly for mapping profiles");
                cfg.AddMaps(Assembly.GetExecutingAssembly());
            }
        });

        var mapper = config.CreateMapper();
        logger.LogInformation("AutoMapper configuration created successfully");

        return new MappingService(logger, mapper);
    }

    /// <summary>
    /// Map from source to destination type
    /// </summary>
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        _logger.LogDebug("Mapping {SourceType} to {DestinationType}",
            typeof(TSource).Name, typeof(TDestination).Name);

        try
        {
            var result = _mapper.Map<TDestination>(source);
            _logger.LogDebug("Mapping completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mapping failed from {SourceType} to {DestinationType}",
                typeof(TSource).Name, typeof(TDestination).Name);
            throw;
        }
    }

    /// <summary>
    /// Map asynchronously (for complex mappings)
    /// </summary>
    public Task<TDestination> MapAsync<TSource, TDestination>(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        _logger.LogDebug("Async mapping {SourceType} to {DestinationType}",
            typeof(TSource).Name, typeof(TDestination).Name);

        // AutoMapper doesn't have native async support, but we can wrap for consistency
        return Task.FromResult(_mapper.Map<TDestination>(source));
    }
}

/// <summary>
/// Base class for AutoMapper profiles with common conventions
/// </summary>
public abstract class MappingProfile : Profile
{
    protected MappingProfile()
    {
        // Apply common conventions
        SourceMemberNamingConvention = LowerUnderscoreNamingConvention.Instance;
        DestinationMemberNamingConvention = PascalCaseNamingConvention.Instance;
    }
}
