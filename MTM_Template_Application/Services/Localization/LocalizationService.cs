using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MTM_Template_Application.Models.Localization;

namespace MTM_Template_Application.Services.Localization;

/// <summary>
/// Localization service - Culture switching, resource loading, missing translation tracking
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    private readonly MissingTranslationHandler _missingTranslationHandler;
    private readonly CultureProvider _cultureProvider;
    private CultureInfo _currentCulture;
    private readonly List<string> _supportedCultures;

    public event EventHandler<LanguageChangedEventArgs>? OnLanguageChanged;

    public LocalizationService(
        MissingTranslationHandler missingTranslationHandler,
        CultureProvider cultureProvider)
    {
        ArgumentNullException.ThrowIfNull(missingTranslationHandler);
        ArgumentNullException.ThrowIfNull(cultureProvider);

        _translations = new Dictionary<string, Dictionary<string, string>>();
        _missingTranslationHandler = missingTranslationHandler;
        _cultureProvider = cultureProvider;
        _currentCulture = cultureProvider.GetCurrentCulture();
        _supportedCultures = new List<string> { "en-US", "es-ES", "fr-FR", "de-DE" };

        InitializeTranslations();
    }

    public string GetString(string key, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(key);

        var cultureName = _currentCulture.Name;
        
        if (_translations.TryGetValue(cultureName, out var cultureTranslations) &&
            cultureTranslations.TryGetValue(key, out var translation))
        {
            return args.Length > 0 ? string.Format(translation, args) : translation;
        }

        // Try fallback culture
        var fallbackCulture = _cultureProvider.GetFallbackCulture(_currentCulture);
        if (fallbackCulture != null && 
            _translations.TryGetValue(fallbackCulture.Name, out var fallbackTranslations) &&
            fallbackTranslations.TryGetValue(key, out var fallbackTranslation))
        {
            _missingTranslationHandler.ReportMissing(key, _currentCulture.Name, fallbackTranslation);
            return args.Length > 0 ? string.Format(fallbackTranslation, args) : fallbackTranslation;
        }

        // Return key as fallback
        _missingTranslationHandler.ReportMissing(key, _currentCulture.Name, key);
        return $"[{key}]";
    }

    public void SetCulture(string cultureName)
    {
        ArgumentNullException.ThrowIfNull(cultureName);

        try
        {
            var newCulture = new CultureInfo(cultureName);
            var oldCulture = _currentCulture;
            _currentCulture = newCulture;

            CultureInfo.CurrentCulture = newCulture;
            CultureInfo.CurrentUICulture = newCulture;

            OnLanguageChanged?.Invoke(this, new LanguageChangedEventArgs
            {
                OldCulture = oldCulture.Name,
                NewCulture = newCulture.Name
            });
        }
        catch (CultureNotFoundException ex)
        {
            throw new ArgumentException($"Culture '{cultureName}' is not supported", nameof(cultureName), ex);
        }
    }

    public List<string> GetSupportedCultures()
    {
        return _supportedCultures.ToList();
    }

    private void InitializeTranslations()
    {
        // English
        _translations["en-US"] = new Dictionary<string, string>
        {
            ["app.title"] = "MTM Template Application",
            ["splash.loading"] = "Loading...",
            ["splash.stage0"] = "Initializing",
            ["splash.stage1"] = "Loading Services",
            ["splash.stage2"] = "Starting Application"
        };

        // Spanish
        _translations["es-ES"] = new Dictionary<string, string>
        {
            ["app.title"] = "Aplicación de Plantilla MTM",
            ["splash.loading"] = "Cargando...",
            ["splash.stage0"] = "Inicializando",
            ["splash.stage1"] = "Cargando Servicios",
            ["splash.stage2"] = "Iniciando Aplicación"
        };
    }
}

public class LanguageChangedEventArgs : EventArgs
{
    public string OldCulture { get; set; } = string.Empty;
    public string NewCulture { get; set; } = string.Empty;
}
