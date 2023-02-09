namespace UserListsAPI.ExternalApi;

public abstract class ItemHttpClientBase
{
  private readonly ILogger _logger;
  private readonly HttpClient _client = new();
  protected readonly string _apiUrl;
  protected readonly string _apiKey;

  public ItemHttpClientBase(ILogger logger, IConfiguration configuration, string itemType)
  {
    _logger = logger;
    _apiUrl = configuration.GetValue<string>($"{itemType}:ApiUrl");
    _apiKey = configuration.GetValue<string>($"{itemType}:ApiKey");
    _client.DefaultRequestHeaders.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("en", 1));
  }

  protected async Task<string?> SendExternalApiRequest(string requestUrl)
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
      return default;
    }
    return httpResponseBody;
  }
}
