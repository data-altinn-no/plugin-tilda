using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dan.Tilda.Models.Audits;
using Dan.Tilda.Models.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Extensions;

public static class HttpClientExtensions
{
    public static async Task<T> GetData<T>(this HttpClient client, string url, string sourceOrgNo, ILogger logger, string mpToken, string requestor) where T : IAuditList, new()
    {
        //For local test purposes
        if (string.IsNullOrEmpty(mpToken))
        {
            mpToken = "NOT SET";
        }

        var resultList = new T();

        using var t = logger.Timer($"{sourceOrgNo}-retrieval");

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("Authorization", "bearer " + mpToken);

            logger.LogInformation(
                "Data retrieval started from sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor}",
                sourceOrgNo, url, requestor);
            var result = await client.SendAsync(request);
            logger.LogInformation(
                "Data retrieval completed from sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} status={status}",
                sourceOrgNo, url, requestor, t.ElapsedMilliseconds, "ok");

            if (result.IsSuccessStatusCode)
            {
                var body = await result.Content.ReadAsStringAsync();
                resultList = JsonConvert.DeserializeObject<T>(body);
                if (resultList == null)
                {
                    resultList = new T();
                    resultList.SetStatusAndTextAndOwner(
                        $"OK (empty response with 200). ElapsedMs: {t.ElapsedMilliseconds}", StatusEnum.Ok,
                        sourceOrgNo);
                    logger.LogWarning(
                        "Data retrieval completed from sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} status={status}",
                        sourceOrgNo, url, requestor, t.ElapsedMilliseconds, "okwarn");
                }
                else
                    resultList.SetStatusAndTextAndOwner($"OK. ElapsedMs: {t.ElapsedMilliseconds}", StatusEnum.Ok,
                        sourceOrgNo);
            }
            else
            {
                resultList.SetStatusAndTextAndOwner(
                    $"Failed: {result.ReasonPhrase}. ElapsedMs: {t.ElapsedMilliseconds}", StatusEnum.Failed,
                    sourceOrgNo);
                logger.LogWarning(
                    "Data retrieval failed gracefully sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} statusCode={statusCode} reasonPhrase={reasonPhrase} status={status}",
                    sourceOrgNo, url, requestor, t.ElapsedMilliseconds, result.StatusCode.ToString(),
                    result.ReasonPhrase, "softfail"
                );
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            logger.LogError("Timeout when fetching data sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} ex={ex} message={message} status={status}",
                sourceOrgNo, url, requestor, t.ElapsedMilliseconds, ex.GetType().Name, ex.Message, "hardfail");
        }
        catch (Exception ex)
        {
            resultList ??= new T();
            resultList.SetStatusAndTextAndOwner(
                $"Failed to retrieve data from {sourceOrgNo} with exception {ex.GetType().Name}: {ex.Message}",
                StatusEnum.Failed, sourceOrgNo);

            logger.LogError(
                "Data retrieval failed with exception sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} ex={ex} message={message} status={status}",
                sourceOrgNo, url, requestor, t.ElapsedMilliseconds, ex.GetType().Name, ex.Message, "hardfail"
            );
        }

        return resultList;
    }

    public static async Task<byte[]> GetPdfreport(this HttpClient client, string url, string sourceOrgNo, ILogger logger, string mpToken, string requestor)
    {
        //For local test purposes
        if (string.IsNullOrEmpty(mpToken))
        {
            mpToken = "NOT SET";
        }

        using var t = logger.Timer($"{sourceOrgNo}-retrieval");

        byte[] result = null;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("Accept", "application/pdf");
            request.Headers.TryAddWithoutValidation("Authorization", "bearer " + mpToken);

            logger.LogInformation("Data retrieval started from sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor}",
                sourceOrgNo, url, requestor);
            var responseMessage = await client.SendAsync(request);
            logger.LogInformation(
                "Data retrieval completed from sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} status={status}",
                sourceOrgNo, url, requestor, t.ElapsedMilliseconds, "ok");

            if (responseMessage.IsSuccessStatusCode)
            {
                result = await responseMessage.Content.ReadAsByteArrayAsync();


                if (result.Length == 0)
                {
                    logger.LogWarning(
                        "Data retrieval completed from sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} status={status}",
                        sourceOrgNo, url, requestor, t.ElapsedMilliseconds, "okwarn");
                }
            }
            else
            {
                logger.LogWarning(
                    "Data retrieval failed gracefully sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} statusCode={statusCode} reasonPhrase={reasonPhrase} status={status}",
                    sourceOrgNo, url, requestor, t.ElapsedMilliseconds, responseMessage.StatusCode.ToString(), responseMessage.ReasonPhrase, "softfail"
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Data retrieval failed with exception sourceOrgNo={sourceOrgNo} on url={url} from requestor={requestor} elapsedMs={elapsedMs} ex={ex} message={message} status={status}",
                sourceOrgNo, url, requestor, t.ElapsedMilliseconds, ex.GetType().Name, ex.Message, "hardfail"
            );
        }

        return result;
    }
}
