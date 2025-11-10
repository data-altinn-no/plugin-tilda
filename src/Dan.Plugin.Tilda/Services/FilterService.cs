using System.Collections.Generic;
using System.Linq;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Services;

public interface IFilterService
{
    IAuditList FilterAuditList(IAuditList resultList, List<TildaRegistryEntry> orgs);
}

public class FilterService : IFilterService
{
    public IAuditList FilterAuditList(IAuditList resultList, List<TildaRegistryEntry> orgs)
    {
        // *** TEMPORARY TILDA FILTERING ***
        // We need to temporarily remove _all_ data from the result list if orgForm is ENK
        // For all other orgforms
        // - remove meldingTilAnnenMyndighet
        // - remove tilsynsnotater
        // - remove kontaktperson name
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

        bool IsEnk(string orgNo)
        {
            return orgs.Any(x => x.OrganizationNumber == orgNo && x.OrganisationForm == "ENK");
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

            case NPDIDAuditReportList filteredResultList:
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
