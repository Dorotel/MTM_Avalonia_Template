using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MTM_Template_Application.Services.Visual;

/// <summary>
/// Validates Visual ERP API Toolkit commands against whitelist and citation requirements.
/// Enforces read-only access per Constitution Principle VIII.
/// </summary>
public class VisualApiWhitelistValidator : IVisualApiWhitelistValidator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<VisualApiWhitelistValidator> _logger;
    private readonly HashSet<string> _allowedCommands;
    private readonly bool _requireCitation;
    private readonly Regex _citationPattern;

    /// <summary>
    /// Initializes a new instance of the <see cref="VisualApiWhitelistValidator"/> class.
    /// </summary>
    /// <param name="configuration">Configuration provider for Visual API settings.</param>
    /// <param name="logger">Logger instance for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown if configuration or logger is null.</exception>
    public VisualApiWhitelistValidator(
        IConfiguration configuration,
        ILogger<VisualApiWhitelistValidator> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        _configuration = configuration;
        _logger = logger;

        // Load whitelist from appsettings.json
        var commandList = _configuration.GetSection("Visual:AllowedCommands").Get<string[]>();
        _allowedCommands = commandList != null
            ? new HashSet<string>(commandList, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        _requireCitation = _configuration.GetValue<bool>("Visual:RequireCitation", true);

        // Citation format: "Reference-{FileName} - {Chapter/Section/Page}"
        _citationPattern = new Regex(
            @"^Reference-[\w\s._-]+ - [\w\s/,.\-]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        _logger.LogInformation(
            "VisualApiWhitelistValidator initialized with {CommandCount} allowed commands. Citation required: {RequireCitation}",
            _allowedCommands.Count,
            _requireCitation);
    }

    /// <summary>
    /// Determines if a Visual API command is allowed based on the whitelist.
    /// </summary>
    /// <param name="command">The command name to validate (e.g., "GET_PART_DETAILS").</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if command is allowed, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if command is null or whitespace.</exception>
    public Task<bool> IsCommandAllowedAsync(string command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Command cannot be null or whitespace.", nameof(command));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var isAllowed = _allowedCommands.Contains(command);

        if (isAllowed)
        {
            _logger.LogDebug("Command '{Command}' is whitelisted - ALLOWED", command);
        }
        else
        {
            _logger.LogWarning("Command '{Command}' is NOT whitelisted - BLOCKED", command);
        }

        return Task.FromResult(isAllowed);
    }

    /// <summary>
    /// Validates a citation against the required format.
    /// </summary>
    /// <param name="citation">The citation string to validate.</param>
    /// <returns>True if citation matches required format, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if citation is null or whitespace when citations are required.</exception>
    public bool ValidateCitationFormat(string? citation)
    {
        if (!_requireCitation)
        {
            _logger.LogDebug("Citations not required - validation skipped");
            return true;
        }

        if (string.IsNullOrWhiteSpace(citation))
        {
            _logger.LogWarning("Citation is required but was not provided or is empty");
            return false;
        }

        var isValid = _citationPattern.IsMatch(citation);

        if (isValid)
        {
            _logger.LogDebug("Citation '{Citation}' matches required format", citation);
        }
        else
        {
            _logger.LogWarning("Citation '{Citation}' does NOT match required format 'Reference-{{FileName}} - {{Chapter/Section/Page}}'", citation);
        }

        return isValid;
    }

    /// <summary>
    /// Validates both command whitelist and citation format in a single operation.
    /// </summary>
    /// <param name="command">The command name to validate.</param>
    /// <param name="citation">The citation string to validate.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if both validations pass, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if command is null or whitespace.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if command is not whitelisted.</exception>
    public async Task<bool> ValidateCommandAsync(
        string command,
        string? citation,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Command cannot be null or whitespace.", nameof(command));
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Check command whitelist
        var isCommandAllowed = await IsCommandAllowedAsync(command, cancellationToken);
        if (!isCommandAllowed)
        {
            _logger.LogError("Unauthorized Visual API command attempted: '{Command}'", command);
            throw new UnauthorizedAccessException(
                $"Command '{command}' is not in the Visual API whitelist. Only read-only commands are permitted.");
        }

        // Check citation format
        var isCitationValid = ValidateCitationFormat(citation);
        if (!isCitationValid)
        {
            _logger.LogError("Invalid or missing citation for command '{Command}'", command);
            return false;
        }

        _logger.LogInformation(
            "Visual API command '{Command}' validated successfully with citation '{Citation}'",
            command,
            citation);

        return true;
    }

    /// <summary>
    /// Gets the list of all allowed commands.
    /// </summary>
    /// <returns>Read-only collection of allowed command names.</returns>
    public IReadOnlyCollection<string> GetAllowedCommands()
    {
        return _allowedCommands.ToList().AsReadOnly();
    }
}
