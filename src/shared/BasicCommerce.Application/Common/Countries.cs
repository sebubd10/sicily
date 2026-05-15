using System.Globalization;

namespace BasicCommerce.Application.Common;

public static class Countries
{
    public static readonly IReadOnlyDictionary<string, string> All;

    static Countries()
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                var region = new RegionInfo(ci.Name);
                dict.TryAdd(region.TwoLetterISORegionName, region.EnglishName);
            }
            catch { /* unsupported region — skip */ }
        }
        All = dict.OrderBy(kv => kv.Value)
                  .ToDictionary(kv => kv.Key, kv => kv.Value,
                      StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsValid(string? code) =>
        !string.IsNullOrWhiteSpace(code) && All.ContainsKey(code);
}
