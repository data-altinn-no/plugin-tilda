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

            if (includeSubunits)
            {
                throw new Exception("inkluderUnderenheter er ikke støttet ennå :)");
            }

            return new TildaParameters(fromDateTime, toDateTime, npdid, false, sourceFilter, identifier, filter, year, month);
        }
    }
}

