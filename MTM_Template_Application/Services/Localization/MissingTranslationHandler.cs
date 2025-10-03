using System;
using System.Collections.Concurrent;
using MTM_Template_Application.Models.Localization;

namespace MTM_Template_Application.Services.Localization;

/// <summary>
/// Log missing translations and report to telemetry
/// </summary>
public class MissingTranslationHandler
{
    private readonly ConcurrentDictionary<string, MissingTranslation> _missingTranslations;

    public MissingTranslationHandler()
    {
        _missingTranslations = new ConcurrentDictionary<string, MissingTranslation>();
    }

    public void ReportMissing(string key, string culture, string fallbackValue)
    {
        var compositeKey = $"{culture}:{key}";
        
        _missingTranslations.AddOrUpdate(
            compositeKey,
            _ => new MissingTranslation
            {
                Key = key,
                Culture = culture,
                FallbackValue = fallbackValue,
                ReportedAt = DateTimeOffset.UtcNow,
                Frequency = 1
            },
            (_, existing) =>
            {
                existing.Frequency++;
                return existing;
            });
    }

    public MissingTranslation[] GetMissingTranslations()
    {
        return _missingTranslations.Values.ToArray();
    }
}
