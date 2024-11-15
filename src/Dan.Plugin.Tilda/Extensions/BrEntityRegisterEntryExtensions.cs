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

        // Need to check if any of the potential up to 3 nace codes match
        // 1. If nace-parameter is null, condition breaks and we don't check it.
        // 2. Since nace is not null, we need to find at least one match out of three codes:
        //   a. If code is null, we don't check for match and check the next code
        //   b. If code does not start with nace, we check the next code
        // 3. If having gone through all codes and not found match, will return false, otherwise
        //    go on to next checks (currently no further checks at the time of writing, but keeping it this
        //    way for future extensibility)
        // See https://www.ssb.no/klass/klassifikasjoner/6 for explanation of nace, and why we check for starts with
        // instead of equals.
        var code1 = brEntity?.Naeringskode1?.Kode;
        var code2 = brEntity?.Naeringskode2?.Kode;
        var code3 = brEntity?.Naeringskode3?.Kode;
        if (tildaParameters.nace != null &&
            (code1 == null || !code1.StartsWith(tildaParameters.nace)) &&
            (code2 == null || !code2.StartsWith(tildaParameters.nace)) &&
            (code3 == null || !code3.StartsWith(tildaParameters.nace)))
        {
            return false;
        }

        return true;
    }
}
