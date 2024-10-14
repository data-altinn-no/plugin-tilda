using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.NewtonsoftJson;
using Dan.Common;
using Dan.Plugin.Tilda.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Clients;

public interface IAltinnEventClient
{
    Task SendEvent(CloudEvent cloudEvent);
    Task CreateSubscription(string endpoint, string resourceFilter);
}

public class AltinnEventClient : IAltinnEventClient
{
    private readonly string _altinnEventsUrl;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AltinnEventClient> _logger;

    public AltinnEventClient(
        IOptions<Settings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<AltinnEventClient> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(Constants.SafeHttpClient);
        _altinnEventsUrl = settings.Value.AltinnEventsBaseUrl;
    }

    public async Task SendEvent(CloudEvent cloudEvent)
    {
        var targetUrl = $"{_altinnEventsUrl}/events/api/v1/events";
        try
        {
            CloudEventFormatter formatter = new JsonEventFormatter();
            var cloudEventContent = cloudEvent.ToHttpContent(ContentMode.Structured, formatter);
            var response = await _httpClient.PostAsync(targetUrl, cloudEventContent);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Error submitting cloud event to Altinn Events. Message: {errorMessage}\nUrl: {targetUrl}"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to POST event {EventId} to {ApiEventsEndpoint}",
                cloudEvent.Id,
                targetUrl);
            throw;
        }
    }

    public async Task CreateSubscription(string endpoint, string resourceFilter)
    {
        var targetUrl = $"{_altinnEventsUrl}/events/api/v1/subscriptions";
        try
        {
            var requestBody = new CreateSubscription
            {
                ResourceFilter = resourceFilter,
                Endpoint = endpoint
            };
            var jsonBody = JsonConvert.SerializeObject(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Error creating subscription in Altinn events. Message: {errorMessage}\nUrl: {targetUrl}"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to POST subscription to {ApiEventsEndpoint}",
                targetUrl);
            throw;
        }
    }
}

// TODO: Move
public class CreateSubscription
{
    [JsonProperty("endpoint")]
    public string Endpoint { get; set; }

    [JsonProperty("resourceFilter")]
    public string ResourceFilter { get; set; }
}
