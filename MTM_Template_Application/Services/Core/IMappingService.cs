using System.Threading.Tasks;

namespace MTM_Template_Application.Services.Core;

/// <summary>
/// Object mapping service using AutoMapper
/// </summary>
public interface IMappingService
{
    /// <summary>
    /// Map from source to destination type
    /// </summary>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Map asynchronously (for complex mappings)
    /// </summary>
    Task<TDestination> MapAsync<TSource, TDestination>(TSource source);
}
