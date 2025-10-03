namespace MTM_Template_Application.Models.Diagnostics;

/// <summary>
/// Status of a diagnostic check
/// </summary>
public enum DiagnosticStatus
{
    /// <summary>
    /// Check passed successfully
    /// </summary>
    Passed,

    /// <summary>
    /// Check passed with warnings
    /// </summary>
    Warning,

    /// <summary>
    /// Check failed
    /// </summary>
    Failed
}
