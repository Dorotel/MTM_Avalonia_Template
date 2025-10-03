using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;

namespace MTM_Template_Application.Services.Core;

/// <summary>
/// Mapping service with AutoMapper integration and profile discovery
/// </summary>
public class MappingService : IMappingService
{
    private readonly IMapper _mapper;

    public MappingService(IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        _mapper = mapper;
    }

    /// <summary>
    /// Create MappingService with auto-discovered profiles
    /// </summary>
    public static MappingService Create(params Assembly[] assemblies)
    {
        var config = new MapperConfiguration(cfg =>
        {
            // Auto-discover profiles from assemblies
            if (assemblies.Length > 0)
            {
                cfg.AddMaps(assemblies);
            }
            else
            {
                // Default: scan current assembly
                cfg.AddMaps(Assembly.GetExecutingAssembly());
            }
        });

        var mapper = config.CreateMapper();
        return new MappingService(mapper);
    }

    /// <summary>
    /// Map from source to destination type
    /// </summary>
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return _mapper.Map<TDestination>(source);
    }

    /// <summary>
    /// Map asynchronously (for complex mappings)
    /// </summary>
    public Task<TDestination> MapAsync<TSource, TDestination>(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
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
