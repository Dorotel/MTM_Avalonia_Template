using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Secrets;

namespace MTM_Template_Application.Services.DataLayer;

/// <summary>
/// Visual API client - HTTP client wrapper, command whitelist enforcement, authentication
/// </summary>
public class VisualApiClient : IVisualApiClient
{
    private readonly ILogger<VisualApiClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly HashSet<string> _whitelistedCommands;
    private readonly string _baseUrl;
    private readonly IAuthenticationProvider? _authenticationProvider;

    public VisualApiClient(
        ILogger<VisualApiClient> logger,
        HttpClient httpClient,
        string baseUrl,
        IEnumerable<string> whitelistedCommands,
        IAuthenticationProvider? authenticationProvider = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(baseUrl);
        ArgumentNullException.ThrowIfNull(whitelistedCommands);

        _logger = logger;

        _logger = logger;
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
        _whitelistedCommands = new HashSet<string>(whitelistedCommands, StringComparer.OrdinalIgnoreCase);
        _authenticationProvider = authenticationProvider;

        _logger.LogInformation("VisualApiClient initialized. BaseUrl: {BaseUrl}, Whitelisted commands: {Count}",
            _baseUrl, _whitelistedCommands.Count);
    }

    /// <summary>
    /// Execute a whitelisted Visual API command
    /// </summary>
    public async Task<T?> ExecuteCommandAsync<T>(string command, Dictionary<string, object> parameters)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(parameters);

        _logger.LogInformation("Executing Visual API command: {Command}", command);

        // Enforce whitelist
        if (!_whitelistedCommands.Contains(command))
        {
            _logger.LogError("Command {Command} is not whitelisted", command);
            throw new UnauthorizedAccessException($"Command '{command}' is not whitelisted for Visual API access");
        }

        _logger.LogDebug("Command {Command} is whitelisted, proceeding with execution", command);

        _logger.LogDebug("Command {Command} is whitelisted, proceeding with execution", command);

        // Add authentication if available
        if (_authenticationProvider != null)
        {
            _logger.LogDebug("Adding authentication token to request");
            var token = await _authenticationProvider.GetAuthenticationTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Build request
        var requestUrl = $"{_baseUrl}/api/{command}";
        _logger.LogDebug("Request URL: {Url}", requestUrl);

        var jsonContent = JsonSerializer.Serialize(parameters);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        // Execute request
        try
        {
            _logger.LogDebug("Sending POST request to Visual API");
            var response = await _httpClient.PostAsync(requestUrl, content);
            response.EnsureSuccessStatusCode();

            // Parse response
            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                _logger.LogDebug("Received empty response for command: {Command}", command);
                return default;
            }

            _logger.LogInformation("Command {Command} executed successfully", command);
            return JsonSerializer.Deserialize<T>(responseContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for command: {Command}", command);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response for command: {Command}", command);
            throw;
        }
    }

    /// <summary>
    /// Check if Visual server is available
    /// </summary>
    public async Task<bool> IsServerAvailable()
    {
        _logger.LogDebug("Checking Visual server availability");
        try
        {
            var healthCheckUrl = $"{_baseUrl}/health";
            var response = await _httpClient.GetAsync(healthCheckUrl);
            var isAvailable = response.IsSuccessStatusCode;
            _logger.LogInformation("Visual server availability check: {Available}", isAvailable);
            return isAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Visual server availability check failed");
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
