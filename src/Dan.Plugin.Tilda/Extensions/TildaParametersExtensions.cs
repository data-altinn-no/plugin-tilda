using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Extensions;

public static class TildaParametersExtensions
{
    public static bool HasGeoSearchParams(this TildaParameters parameters)
    {
        if (parameters is null)
        {
            return false;
        }

        return !string.IsNullOrEmpty(parameters.postcode) ||
               !string.IsNullOrEmpty(parameters.municipalityNumber) ||
               !string.IsNullOrEmpty(parameters.nace);
    }
}
