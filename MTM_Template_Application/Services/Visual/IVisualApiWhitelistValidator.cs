using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MTM_Template_Application.Services.Visual;

/// <summary>
/// Interface for validating Visual ERP API Toolkit commands against whitelist.
/// Enforces read-only access per Constitution Principle VIII.
/// </summary>
public interface IVisualApiWhitelistValidator
{
    /// <summary>
    /// Determines if a Visual API command is allowed based on the whitelist.
    /// </summary>
    /// <param name="command">The command name to validate (e.g., "GET_PART_DETAILS").</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if command is allowed, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if command is null or whitespace.</exception>
    Task<bool> IsCommandAllowedAsync(string command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a citation against the required format.
    /// Format: "Reference-{FileName} - {Chapter/Section/Page}"
    /// </summary>
    /// <param name="citation">The citation string to validate.</param>
    /// <returns>True if citation matches required format or citations are not required, false otherwise.</returns>
    bool ValidateCitationFormat(string? citation);

    /// <summary>
    /// Validates both command whitelist and citation format in a single operation.
    /// </summary>
    /// <param name="command">The command name to validate.</param>
    /// <param name="citation">The citation string to validate.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if both validations pass, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if command is null or whitespace.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if command is not whitelisted.</exception>
    Task<bool> ValidateCommandAsync(string command, string? citation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of all allowed commands.
    /// </summary>
    /// <returns>Read-only collection of allowed command names.</returns>
    IReadOnlyCollection<string> GetAllowedCommands();
}
