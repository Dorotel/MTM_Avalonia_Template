using System.Net.Http;
using System.Threading.Tasks;

namespace MTM_Template_Application.Services.DataLayer;

/// <summary>
/// Generic HTTP API client with retry and circuit breaker
/// </summary>
public interface IHttpApiClient
{
    /// <summary>
    /// Send GET request
    /// </summary>
    Task<T?> GetAsync<T>(string url);

    /// <summary>
    /// Send POST request
    /// </summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data);

    /// <summary>
    /// Send PUT request
    /// </summary>
    Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data);

    /// <summary>
    /// Send DELETE request
    /// </summary>
    Task<HttpResponseMessage> DeleteAsync(string url);
}
