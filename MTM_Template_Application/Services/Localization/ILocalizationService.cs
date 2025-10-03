using System;
using System.Collections.Generic;

namespace MTM_Template_Application.Services.Localization;

/// <summary>
/// Localization and culture management service
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Get localized string
    /// </summary>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Set current culture
    /// </summary>
    void SetCulture(string cultureName);

    /// <summary>
    /// Get list of supported cultures
    /// </summary>
    List<string> GetSupportedCultures();

    /// <summary>
    /// Event raised when language changes
    /// </summary>
    event EventHandler<LanguageChangedEventArgs>? OnLanguageChanged;
}

/// <summary>
/// Language changed event arguments
/// </summary>
public class LanguageChangedEventArgs : EventArgs
{
    public string OldCulture { get; init; } = string.Empty;
    public string NewCulture { get; init; } = string.Empty;
}
