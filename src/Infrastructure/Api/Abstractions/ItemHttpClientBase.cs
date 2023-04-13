using Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Api.Abstractions;

public abstract class ItemHttpClientBase
{
    private readonly ILogger _logger;
    private readonly HttpClient _client = new();
    protected readonly string _apiUrl;
    protected readonly string _apiKey;

    public ItemHttpClientBase(ILogger logger, IConfiguration configuration, string itemType)
    {
        _logger = logger;
        _apiUrl = configuration.GetValue<string>($"{itemType}:ApiUrl") ?? throw new ConfigException($"'{itemType}:ApiUrl' was not found in configuration");
        _apiKey = configuration.GetValue<string>($"{itemType}:ApiKey") ?? throw new ConfigException($"'{itemType}:ApiKey' was not found in configuration");
    }

    protected async Task<string?> SendApiRequestAsync(string requestUrl)
    {
        HttpResponseMessage httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        string httpResponseBody = string.Empty;
        try
        {
            _logger.LogInformation("Request URL: {requestUrl}", requestUrl);
            httpResponse = await _client.GetAsync(requestUrl);
            httpResponse.EnsureSuccessStatusCode();
            httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Http request {requestMessage} returned {statusCode}", httpResponse.RequestMessage, httpResponse.StatusCode);
            _logger.LogWarning("Exception: {exceptionMessage}", ex.Message);
            throw new AppException($"Request {requestUrl} failed");
        }
        return httpResponseBody;
    }
}