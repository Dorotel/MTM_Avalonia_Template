namespace MTM_Template_Application.Models.Core;

/// <summary>
/// Metadata for validation rules
/// </summary>
public class ValidationRuleMetadata
{
    /// <summary>
    /// Name of the validation rule
    /// </summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>
    /// Property being validated
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Severity: Error, Warning, Information
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Error message template
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Type of validator used
    /// </summary>
    public string ValidatorType { get; set; } = string.Empty;
}
