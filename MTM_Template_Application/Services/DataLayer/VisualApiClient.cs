using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Template_Application.Services.Secrets;

namespace MTM_Template_Application.Services.DataLayer;

/// <summary>
/// Visual API client - HTTP client wrapper, command whitelist enforcement, authentication
/// </summary>
public class VisualApiClient : IVisualApiClient
{
    private readonly HttpClient _httpClient;
    private readonly HashSet<string> _whitelistedCommands;
    private readonly string _baseUrl;
    private readonly IAuthenticationProvider? _authenticationProvider;

    public VisualApiClient(
        HttpClient httpClient,
        string baseUrl,
        IEnumerable<string> whitelistedCommands,
        IAuthenticationProvider? authenticationProvider = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(baseUrl);
        ArgumentNullException.ThrowIfNull(whitelistedCommands);

        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
        _whitelistedCommands = new HashSet<string>(whitelistedCommands, StringComparer.OrdinalIgnoreCase);
        _authenticationProvider = authenticationProvider;
    }

    /// <summary>
    /// Execute a whitelisted Visual API command
    /// </summary>
    public async Task<T?> ExecuteCommandAsync<T>(string command, Dictionary<string, object> parameters)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(parameters);

        // Enforce whitelist
        if (!_whitelistedCommands.Contains(command))
        {
            throw new UnauthorizedAccessException($"Command '{command}' is not whitelisted for Visual API access");
        }

        // Add authentication if available
        if (_authenticationProvider != null)
        {
            var token = await _authenticationProvider.GetAuthenticationTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Build request
        var requestUrl = $"{_baseUrl}/api/{command}";
        var jsonContent = JsonSerializer.Serialize(parameters);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        // Execute request
        var response = await _httpClient.PostAsync(requestUrl, content);
        response.EnsureSuccessStatusCode();

        // Parse response
        var responseContent = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(responseContent);
    }

    /// <summary>
    /// Check if Visual server is available
    /// </summary>
    public async Task<bool> IsServerAvailable()
    {
        try
        {
            var healthCheckUrl = $"{_baseUrl}/health";
            var response = await _httpClient.GetAsync(healthCheckUrl);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get list of whitelisted API commands
    /// </summary>
    public List<string> GetWhitelistedCommands()
    {
        return _whitelistedCommands.ToList();
    }
}

/// <summary>
/// Authentication provider interface for Visual API
/// </summary>
public interface IAuthenticationProvider
{
    /// <summary>
    /// Get authentication token for Visual API
    /// </summary>
    Task<string> GetAuthenticationTokenAsync();
}

/// <summary>
/// Device certificate + user credentials authentication provider
/// </summary>
public class DeviceCertificateAuthenticationProvider : IAuthenticationProvider
{
    private readonly ISecretsService _secretsService;
    private readonly string _deviceCertificateKey;
    private readonly string _userCredentialsKey;

    public DeviceCertificateAuthenticationProvider(
        ISecretsService secretsService,
        string deviceCertificateKey = "VisualDeviceCertificate",
        string userCredentialsKey = "VisualUserCredentials")
    {
        ArgumentNullException.ThrowIfNull(secretsService);

        _secretsService = secretsService;
        _deviceCertificateKey = deviceCertificateKey;
        _userCredentialsKey = userCredentialsKey;
    }

    public async Task<string> GetAuthenticationTokenAsync()
    {
        // Retrieve device certificate
        var deviceCert = await _secretsService.RetrieveSecretAsync(_deviceCertificateKey);
        if (deviceCert == null)
        {
            throw new UnauthorizedAccessException("Device certificate not found");
        }

        // Retrieve user credentials
        var userCreds = await _secretsService.RetrieveSecretAsync(_userCredentialsKey);
        if (userCreds == null)
        {
            throw new UnauthorizedAccessException("User credentials not found");
        }

        // Combine into authentication token (simplified - in production, use proper OAuth/JWT)
        var token = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{deviceCert}:{userCreds}"));

        return token;
    }
}
