using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dan.Common.Extensions;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Extensions;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Services;
using Dan.Tilda.Models.Entities;

namespace Dan.Plugin.Tilda.Functions;

public abstract class AuditFunctionsBase(IBrregService brregService)
{
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
        AccountsInformation accountsInformation = null;

        // Filters out on parameters, currently only on "geo search" params
        if (!brEntity.MatchesFilterParameters(tildaParameters))
        {
            return null;
        }

        if (brEntity.Organisasjonsform.Kode != "ENK")
        {
            accountsInformation = await brregService.GetAnnualTurnoverFromBr(organizationNumber);
        }

        var kofuviAddresses = await brregService.GetKofuviAddresses(organizationNumber);

        var organization = ConvertBRtoTilda(brEntity, accountsInformation);
        if (kofuviAddresses.Count > 0)
        {
            organization.Emails = kofuviAddresses;
        }

        return organization;
    }

    protected async Task<List<TildaRegistryEntry>> GetOrganizationsFromBr(string organizationNumber)
    {
        var result = new List<TildaRegistryEntry>();
        var brResult = await brregService.GetFromBr(organizationNumber, false);
        var brEntity = brResult.First();
        AccountsInformation accountsInformation = null;
        if (string.IsNullOrEmpty(brEntity.OverordnetEnhet) && brEntity.Organisasjonsform.Kode != "ENK")
        {
            accountsInformation = await brregService.GetAnnualTurnoverFromBr(organizationNumber);
        }

        var kofuviAddresses = await brregService.GetKofuviAddresses(organizationNumber);

        var organization = ConvertBRtoTilda(brEntity, accountsInformation);
        if (kofuviAddresses.Count > 0)
        {
            organization.Emails = kofuviAddresses;
        }
        result.Add(organization);
        return result;
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

        return OperationStatus.OK;
    }
}
