using System;
using Dan.Common.Models;
using System.Linq;
using Dan.Common.Extensions;
using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Extensions
{
    public static class EvidenceHarvesterRequestExtension
    {
        public static TildaParameters GetValuesFromParameters(this EvidenceHarvesterRequest req)
        {
            DateTime? fromDateTime = null;
            DateTime? toDateTime = null;

            if (req.TryGetParameter("startdato", out DateTime fromDate))
            {
                fromDateTime = fromDate.ToUniversalTime();
            }

            if (req.TryGetParameter("sluttdato", out DateTime toDate))
            {
                toDateTime = toDate.ToUniversalTime();
            }

            req.TryGetParameter("npdid", out string npdid);
            req.TryGetParameter("tilsynskilder", out string sourceFilter);
            req.TryGetParameter("inkluderUnderenheter", out bool includeSubunits);
            req.TryGetParameter("identifikator", out string identifier);
            req.TryGetParameter("filter", out string filter);
            req.TryGetParameter("aar", out string year);
            req.TryGetParameter("maaned", out string month);
            req.TryGetParameter("postnummer", out string postcode);
            req.TryGetParameter("kommunenummer", out string municipalityNumber);
            req.TryGetParameter("naeringskode", out string nace);

            if (includeSubunits)
                throw new Exception("inkluderUnderenheter er ikke støttet ennå :)");

            return new TildaParameters(fromDateTime, toDateTime, npdid, false, sourceFilter, identifier, filter, year, month, postcode, municipalityNumber, nace);
        }

        // Since Tilda supports NPDID which doesnt follow a strict scheme, we can get requests where scheme is null
        // and that is okay, we just use the id. If scheme is set, then we don't want to use Id, as the Id will be
        // formatted like 'scheme:id', so in that case we just pick the organisationnumber. Tilda doesnt support
        // norwegian ssn (or any individual person) as subject, so don't need to account for that.
        public static string GetTildaSubject(this EvidenceHarvesterRequest req)
        {
            return req.SubjectParty.Scheme is null ? req.SubjectParty.Id : req.SubjectParty.NorwegianOrganizationNumber;
        }
    }
}

