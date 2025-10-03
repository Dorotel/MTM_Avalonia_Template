using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace MTM_Template_Application.Services.Configuration;

/// <summary>
/// Persists configuration profiles to local storage
/// </summary>
public class ConfigurationPersistence
{
    private readonly ILogger<ConfigurationPersistence> _logger;
    private readonly string _storageDirectory;

    public ConfigurationPersistence(ILogger<ConfigurationPersistence> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        
        // Use AppData for Windows, ~/.config for Linux/Mac
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _storageDirectory = Path.Combine(appData, "MTM_Template", "Config");
        
        Directory.CreateDirectory(_storageDirectory);
    }

    /// <summary>
    /// Save configuration profile to local storage
    /// </summary>
    public async Task SaveProfileAsync(ConfigurationProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var filePath = Path.Combine(_storageDirectory, $"{profile.ProfileName}.json");
        
        try
        {
            var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);
            
            _logger.LogInformation("Configuration profile {ProfileName} saved to {Path}", 
                profile.ProfileName, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration profile {ProfileName}", profile.ProfileName);
            throw;
        }
    }

    /// <summary>
    /// Load configuration profile from local storage
    /// </summary>
    public async Task<ConfigurationProfile?> LoadProfileAsync(string profileName)
    {
        ArgumentNullException.ThrowIfNull(profileName);

        var filePath = Path.Combine(_storageDirectory, $"{profileName}.json");
        
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Configuration profile {ProfileName} not found at {Path}", 
                profileName, filePath);
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var profile = JsonSerializer.Deserialize<ConfigurationProfile>(json);
            
            _logger.LogInformation("Configuration profile {ProfileName} loaded from {Path}", 
                profileName, filePath);
            
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration profile {ProfileName}", profileName);
            return null;
        }
    }

    /// <summary>
    /// Get all available profile names
    /// </summary>
    public Task<IEnumerable<string>> GetProfileNamesAsync()
    {
        try
        {
            var files = Directory.GetFiles(_storageDirectory, "*.json");
            var profileNames = Array.ConvertAll(files, f => Path.GetFileNameWithoutExtension(f));
            
            _logger.LogDebug("Found {Count} configuration profiles", profileNames.Length);
            
            return Task.FromResult<IEnumerable<string>>(profileNames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enumerate configuration profiles");
            return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        }
    }

    /// <summary>
    /// Delete a configuration profile
    /// </summary>
    public Task DeleteProfileAsync(string profileName)
    {
        ArgumentNullException.ThrowIfNull(profileName);

        var filePath = Path.Combine(_storageDirectory, $"{profileName}.json");
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Configuration profile {ProfileName} deleted", profileName);
        }
        else
        {
            _logger.LogWarning("Configuration profile {ProfileName} not found for deletion", profileName);
        }

        return Task.CompletedTask;
    }
}
