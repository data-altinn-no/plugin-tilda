using System;
using System.Collections.Generic;
using System.Linq;
using Dan.Plugin.Tilda.Models;
using Dan.Tilda.Models.Audits;
using Dan.Tilda.Models.Audits.Coordination;
using Dan.Tilda.Models.Audits.NPDID;
using Dan.Tilda.Models.Audits.Report;
using Dan.Tilda.Models.Audits.Trend;
using Dan.Tilda.Models.Entities;

namespace Dan.Plugin.Tilda.Services;

public interface IFilterService
{
    IAuditList FilterAuditList(IAuditList resultList, List<TildaRegistryEntry> orgs, bool orgInfoUnavailable = false);
}

public class FilterService : IFilterService
{
    public IAuditList FilterAuditList(IAuditList resultList, List<TildaRegistryEntry> orgs, bool orgInfoUnavailable = false)
    {
        // *** TEMPORARY TILDA FILTERING ***
        // We need to temporarily remove _all_ data from the result list if orgForm is ENK
        // For all other orgforms
        // - remove meldingTilAnnenMyndighet
        // - remove tilsynsnotater
        // - remove kontaktperson name
        Func<string, bool> IsEnk;

        if (orgInfoUnavailable)
        {
            // Fail closed: we couldn't determine org forms from BR (e.g. a timeout), so we can't
            // tell which control objects are ENK. Treat every one as ENK and strip protected
            // fields rather than risk exposing an ENK's data.
            IsEnk = _ => true;
        }
        else
        {
            if (orgs == null || orgs.Count == 0)
            {
                return resultList;
            }

            var firstOrg = orgs.First();
            // If first organization is a specific test organization, disable filtering
            if (firstOrg.OrganizationNumber == "111111111")
            {
                return resultList;
            }

            var enkOrgs = orgs
                .Where(x => x.OrganisationForm == "ENK")
                .Select(x => x.OrganizationNumber)
                .ToHashSet();

            IsEnk = enkOrgs.Contains;
        }

        switch (resultList)
        {
            case AuditReportList filteredResultList:
                filteredResultList.AuditReports?.ForEach(x =>
                {
                    if (x is null)
                    {
                        return;
                    }

                    x.AuditNotes = null;

                    if (IsEnk(x.ControlObject))
                    {
                        x.NotesAndRemarks?.Clear();
                        x.ControlAttributes = null;
                    }
                });
                return filteredResultList;

            case AuditCoordinationList filteredResultList:
                filteredResultList.AuditCoordinations?.ForEach(x =>
                {
                    if (x is null)
                    {
                        return;
                    }

                    if (IsEnk(x.ControlObject))
                    {
                        x.Alerts?.Clear();
                    }
                });
                return filteredResultList;

            case NpdidAuditReportList filteredResultList:
                filteredResultList.AuditReports?.ForEach(x =>
                {
                    if (x is null)
                    {
                        return;
                    }

                    x.AuditNotes = null;

                    if (IsEnk(x.ControlObject))
                    {
                        x.NotesAndRemarks?.Clear();
                        x.ControlAttributes = null;
                    }
                });
                return filteredResultList;

            case TrendReportList filteredResultList:

                //Remove all ENKs from trendreport list
                filteredResultList.TrendReports = filteredResultList.TrendReports?.Where(x => !IsEnk(x.ControlObject)).ToList();

                return filteredResultList;
        }

        return resultList;
    }
}
