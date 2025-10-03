using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Core;

namespace MTM_Template_Application.Services.Core;

/// <summary>
/// Validation service using FluentValidation
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validate an object
    /// </summary>
    Task<ValidationResult> ValidateAsync<T>(T obj) where T : class;

    /// <summary>
    /// Register a validator for a type
    /// </summary>
    void RegisterValidator<T>(object validator) where T : class;

    /// <summary>
    /// Get validation rule metadata
    /// </summary>
    List<ValidationRuleMetadata> GetRuleMetadata<T>() where T : class;
}

/// <summary>
/// Validation result
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; init; }
    public List<ValidationError> Errors { get; init; } = new();
}

/// <summary>
/// Validation error
/// </summary>
public class ValidationError
{
    public string PropertyName { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
}
