using System;
using System.Web;

namespace Dan.Plugin.Tilda.Utils;

public interface IUriFormatter
{
    string GetUri(
        string baseUri,
        string dataset,
        string organizationNumber,
        string requestor,
        DateTime? fromDate,
        DateTime? toDate,
        string identifier = "",
        string npdid = "");

    string GetUriAll(
        string baseUri,
        string dataset,
        string requestor,
        string month,
        string year,
        string identifier = "",
        string npdid = "",
        string filter = "");
}

public class UriFormatter : IUriFormatter
{
    public string GetUri(string baseUri, string dataset, string organizationNumber, string requestor, DateTime? fromDate, DateTime? toDate, string identifier = "", string npdid = "")
    {
        string apiUrl = $"{baseUri}/{dataset}/{organizationNumber}?requestor={requestor}";

        if (fromDate != null)
        {
            apiUrl += $"&fromDate={((DateTime)fromDate).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", System.Globalization.CultureInfo.CurrentCulture)}";
        }

        if (toDate != null)
        {
            apiUrl += $"&toDate={((DateTime)toDate).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", System.Globalization.CultureInfo.CurrentCulture)}";
        }

        if (!string.IsNullOrEmpty(npdid))
        {
            apiUrl += $"&npdid={npdid}";
        }

        if (!string.IsNullOrEmpty(identifier))
        {
            apiUrl += $"&id={identifier}";
        }

        return apiUrl;
    }

    public string GetUriAll(string baseUri, string dataset, string requestor, string month, string year, string identifier ="",string npdid = "", string filter = "")
    {
        string apiUrl = $"{baseUri}/{dataset}?requestor={requestor}";

        if (!string.IsNullOrEmpty(month))
        {
            apiUrl += $"&maaned={month}";
        }

        if (!string.IsNullOrEmpty(year))
        {
            apiUrl += $"&aar={year}";
        }

        if (!string.IsNullOrEmpty(npdid))
        {
            apiUrl += $"&npdid={npdid}";
        }

        if (!string.IsNullOrEmpty(identifier))
        {
            apiUrl += $"&id={identifier}";
        }

        if (!string.IsNullOrEmpty(filter))
            apiUrl += $"&filter={HttpUtility.UrlEncode(filter)}";

        return apiUrl;
    }
}
