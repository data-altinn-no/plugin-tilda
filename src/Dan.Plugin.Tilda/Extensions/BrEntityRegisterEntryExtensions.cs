using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Extensions;

public static class BrEntityRegisterEntryExtensions
{
    public static bool MatchesFilterParameters(this BREntityRegisterEntry brEntity, TildaParameters tildaParameters)
    {
        // No params; nothing to filter on
        if (tildaParameters == null)
        {
            return true;
        }

        if (tildaParameters.postcode != null && brEntity.Postadresse?.Postnummer != tildaParameters.postcode)
        {
            return false;
        }

        if (tildaParameters.municipalityNumber != null &&
            brEntity.Postadresse?.Kommunenummer != tildaParameters.municipalityNumber)
        {
            return false;
        }

        if (tildaParameters.nace != null &&
            (brEntity.Naeringskode1?.Kode != tildaParameters.nace ||
             brEntity.Naeringskode2?.Kode != tildaParameters.nace ||
             brEntity.Naeringskode3?.Kode != tildaParameters.nace))
        {
            return false;
        }

        return true;
    }
}
