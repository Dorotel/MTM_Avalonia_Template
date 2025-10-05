using System;
using System.Globalization;

namespace MTM_Template_Application.Services.Localization;

/// <summary>
/// Culture provider - Detect OS culture, fallback chain (selected > OS > en-US)
/// </summary>
public class CultureProvider
{
    private readonly CultureInfo _defaultCulture;

    public CultureProvider()
    {
        _defaultCulture = new CultureInfo("en-US");
    }

    public virtual CultureInfo GetCurrentCulture()
    {
        try
        {
            return CultureInfo.CurrentUICulture;
        }
        catch
        {
            return _defaultCulture;
        }
    }

    public virtual CultureInfo? GetFallbackCulture(CultureInfo current)
    {
        // Fallback chain: specific culture > parent culture > en-US
        if (!current.IsNeutralCulture && current.Parent != CultureInfo.InvariantCulture)
        {
            return current.Parent;
        }

        if (!current.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase))
        {
            return _defaultCulture;
        }

        return null;
    }

    public virtual CultureInfo GetOSCulture()
    {
        return CultureInfo.InstalledUICulture;
    }
}
