using System;
using System.Threading;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Polly;
using MTM_Template_Application.Services.DataLayer.Policies;

namespace MTM_Template_Application.Services.DataLayer;

/// <summary>
/// Generic HTTP client with retry/circuit breaker policies
/// </summary>
public class HttpApiClient : IHttpApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AsyncPolicy<HttpResponseMessage> _resiliencePolicy;

    public HttpApiClient(
        HttpClient httpClient,
        ExponentialBackoffPolicy? backoffPolicy = null,
        CircuitBreakerPolicy? circuitBreakerPolicy = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        _httpClient = httpClient;

        // Build resilience policy combining retry + circuit breaker
        var retry = (backoffPolicy ?? new ExponentialBackoffPolicy()).GetPolicy();
        var circuitBreaker = (circuitBreakerPolicy ?? new CircuitBreakerPolicy()).GetPolicy();

        _resiliencePolicy = Policy.WrapAsync(retry, circuitBreaker);
    }

    /// <summary>
    /// Execute HTTP GET request
    /// </summary>
    public async Task<T?> GetAsync<T>(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        var response = await _resiliencePolicy.ExecuteAsync(async () =>
        {
            return await _httpClient.GetAsync(url);
        });

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(content);
    }

    /// <summary>
    /// Execute HTTP POST request
    /// </summary>
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(data);

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _resiliencePolicy.ExecuteAsync(async () =>
        {
            return await _httpClient.PostAsync(url, content);
        });

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TResponse>(responseContent);
    }

    /// <summary>
    /// Execute HTTP PUT request
    /// </summary>
    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(data);

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _resiliencePolicy.ExecuteAsync(async () =>
        {
            return await _httpClient.PutAsync(url, content);
        });

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TResponse>(responseContent);
    }

    /// <summary>
    /// Execute HTTP DELETE request
    /// </summary>
    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        var response = await _resiliencePolicy.ExecuteAsync(async () =>
        {
            return await _httpClient.DeleteAsync(url);
        });

        response.EnsureSuccessStatusCode();
        return response;
    }
}
