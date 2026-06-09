using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dan.Common.Extensions;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Extensions;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Services;
using Dan.Tilda.Models.Entities;
using Dan.Tilda.Models.Enums;
using Microsoft.Extensions.Logging;

namespace Dan.Plugin.Tilda.Functions;

public abstract class AuditFunctionsBase(IBrregService brregService)
{
    // Caps the outbound fan-out when enriching "Alle" results with org info from BR.
    // Each lookup can spawn up to 3 sequential HTTP calls (enheter, regnskap, kofuvi),
    // so unbounded parallelism over hundreds of orgs risks SNAT port exhaustion and
    // rate limiting from BR on cold cache.
    private const int MaxConcurrentBrLookups = 16;

    protected static TildaParameters GetValuesFromParameters(EvidenceHarvesterRequest req)
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
        {
            throw new Exception("inkluderUnderenheter er ikke støttet ennå :)");
        }

        return new TildaParameters(fromDateTime, toDateTime, npdid, false, sourceFilter, identifier, filter, year, month, postcode, municipalityNumber, nace);
    }

    protected async Task<TildaRegistryEntry> GetOrganizationFromBr(string organizationNumber, TildaParameters tildaParameters = null)
    {
        var brResult = await brregService.GetFromBr(organizationNumber, false);
        var brEntity = brResult.First();

        // Filters out on parameters, currently only on "geo search" params
        if (!brEntity.MatchesFilterParameters(tildaParameters))
        {
            return null;
        }

        // Annual turnover and kofuvi addresses are independent lookups, fetch them in parallel
        var accountsTask = brEntity.Organisasjonsform?.Kode != "ENK"
            ? brregService.GetAnnualTurnoverFromBr(organizationNumber)
            : Task.FromResult<AccountsInformation>(null);
        var kofuviTask = brregService.GetKofuviAddresses(organizationNumber);
        await Task.WhenAll(accountsTask, kofuviTask);
        var accountsInformation = accountsTask.Result;
        var kofuviAddresses = kofuviTask.Result;

        var organization = ConvertBRtoTilda(brEntity, accountsInformation);
        if (kofuviAddresses.Count > 0)
        {
            organization.Emails = kofuviAddresses;
        }

        return organization;
    }

    /// <summary>
    /// Looks up org info from BR for a set of organization numbers with bounded parallelism.
    /// Failed lookups are logged and skipped so one failed fetch doesn't break the listing
    /// of the rest of the orgs. Results filtered out by <paramref name="param"/> are excluded.
    /// </summary>
    protected async Task<List<TildaRegistryEntry>> GetOrganizationsFromBrBounded(
        IEnumerable<string> organizationNumbers, TildaParameters param, ILogger logger)
    {
        var results = new ConcurrentBag<TildaRegistryEntry>();
        await Parallel.ForEachAsync(
            organizationNumbers.Distinct(),
            new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrentBrLookups },
            async (organizationNumber, _) =>
            {
                try
                {
                    var entry = await GetOrganizationFromBr(organizationNumber, param);
                    if (entry is not null)
                    {
                        results.Add(entry);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed getting org info from BR for org {OrganizationNumber}: {message}",
                        organizationNumber, ex.Message);
                }
            });

        return results
            .GroupBy(x => x.OrganizationNumber)
            .Select(y => y.First())
            .ToList();
    }

    /// <summary>
    /// Result of looking up the requestor's organization info from BR. Org info is added-value
    /// enrichment, not critical, so a failed/timed-out lookup must never fail the whole request.
    /// When <see cref="OrgInfoUnavailable"/> is true the caller must fail closed in filtering: we
    /// can't tell whether the org is an ENK, so protected fields are stripped rather than exposed.
    /// </summary>
    protected sealed record BrOrganizationsResult(List<TildaRegistryEntry> Organizations, bool OrgInfoUnavailable);

    protected async Task<BrOrganizationsResult> GetOrganizationsFromBr(string organizationNumber, ILogger logger = null)
    {
        try
        {
            var brResult = await brregService.GetFromBr(organizationNumber, false);
            var brEntity = brResult.FirstOrDefault();
            if (brEntity is null)
            {
                return new BrOrganizationsResult([], OrgInfoUnavailable: true);
            }

            // Annual turnover and kofuvi addresses are independent lookups, fetch them in parallel
            var accountsTask = string.IsNullOrEmpty(brEntity.OverordnetEnhet) && brEntity.Organisasjonsform?.Kode != "ENK"
                ? brregService.GetAnnualTurnoverFromBr(organizationNumber)
                : Task.FromResult<AccountsInformation>(null);
            var kofuviTask = brregService.GetKofuviAddresses(organizationNumber);
            await Task.WhenAll(accountsTask, kofuviTask);
            var accountsInformation = accountsTask.Result;
            var kofuviAddresses = kofuviTask.Result;

            var organization = ConvertBRtoTilda(brEntity, accountsInformation);
            if (kofuviAddresses.Count > 0)
            {
                organization.Emails = kofuviAddresses;
            }
            return new BrOrganizationsResult([organization], OrgInfoUnavailable: false);
        }
        catch (Exception ex)
        {
            // BR org info is added-value enrichment, not critical: never fail the whole request on
            // its account (e.g. an ERHttpClient timeout throws TaskCanceledException here). Signal
            // that org info is unavailable so filtering fails closed.
            logger?.LogError(ex, "Failed getting org info from BR for org {OrganizationNumber}: {message}",
                organizationNumber, ex.Message);
            return new BrOrganizationsResult([], OrgInfoUnavailable: true);
        }
    }

    private TildaRegistryEntry ConvertBRtoTilda(BREntityRegisterEntry brResult, AccountsInformation accountsInformation)
    {
        // *** TEMPORARY TILDA FILTERING ***
        if (brResult.Organisasjonsform == null)
        {
            return new TildaRegistryEntry()
            {
                OrganisationForm = "",
                OperationalStatus = OperationStatus.Blank,
                OrganizationNumber = brResult.Organisasjonsnummer.ToString()
            };
        }

        if (brResult.Organisasjonsform.Kode == "TEST")
        {
            return new TildaRegistryEntry()
            {
                OrganisationForm = brResult.Organisasjonsform.Kode,
                OperationalStatus = GetOperationStatus(brResult),
                OrganizationNumber = brResult.Organisasjonsnummer.ToString()
            };
        }

        //remove financial information from ENKs - GDPR
        if (brResult.Organisasjonsform.Kode == "ENK")
        {
            accountsInformation = null;
        }

        var item =  new TildaRegistryEntry()
        {
            OrganisationForm = brResult.Organisasjonsform.Kode,
            Name = brResult.Navn,
            Accounts = accountsInformation,
            BusinessCode = brResult.Naeringskode1?.Kode,
            OperationalStatus = GetOperationStatus(brResult),
            OrganizationNumber = brResult.Organisasjonsnummer.ToString()
        };


        if (brResult.Forretningsadresse != null)
        {
            item.PublicLocationAddress = new ErAddress()
            {
                AddressName = string.Join(",", brResult.Forretningsadresse?.Adressenavn ?? []),
                PostNumber = brResult.Forretningsadresse?.Postnummer,
                PostName = brResult.Forretningsadresse?.Poststed,
                CountyNumber = brResult.Forretningsadresse?.Kommunenummer,
                MunicipalityNumber = brResult.Forretningsadresse?.Kommunenummer
            };
        }
        return item;
    }

    private OperationStatus GetOperationStatus(BREntityRegisterEntry brResult)
    {
        if (brResult.Konkurs)
        {
            return OperationStatus.Konkurs;
        }

        if (brResult.UnderAvvikling)
        {
            return OperationStatus.UnderAvvikling;
        }

        if (brResult.UnderTvangsavviklingEllerTvangsopplosning)
        {
            return OperationStatus.UnderTvangsavviklingEllerTvangsopplosning;
        }

        if (brResult.Slettedato != DateTime.MinValue)
        {
            return OperationStatus.Slettet;
        }

        return OperationStatus.Ok;
    }
}
