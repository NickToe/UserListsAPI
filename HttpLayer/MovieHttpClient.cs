using System.Text.Json;
using UserListsAPI.JsonModels;

namespace UserListsAPI.HttpLayer;

public class MovieHttpClient : ItemHttpClientBase
{
  private readonly ILogger<MovieHttpClient> _logger;

  public MovieHttpClient(ILogger<MovieHttpClient> logger, IConfiguration configuration) : base(logger, configuration, "Movie")
  {
    _logger = logger;
  }

  public async Task<MovieJson?> GetById(string id)
  {
    string requestUrl = $"{_apiUrl}/Title/{_apiKey}/{id}";
    string? httpResponseBody = await SendExternalApiRequest(requestUrl);
    if (string.IsNullOrEmpty(httpResponseBody)) return default(MovieJson);
    return Deserialize(httpResponseBody);
  }

  public async Task<IEnumerable<MovieJsonShort>> GetAllByTitle(string title)
  {
    string requestUrl = $"{_apiUrl}/SearchMovie/{_apiKey}/{title}";
    string? httpResponseBody = await SendExternalApiRequest(requestUrl);
    if(string.IsNullOrEmpty(httpResponseBody)) return Enumerable.Empty<MovieJsonShort>();
    return DeserializeAll(httpResponseBody);
  }

  private IEnumerable<MovieJsonShort> DeserializeAll(string httpResponse)
  {
    string errorMessage = JsonDocument.Parse(httpResponse).RootElement.GetProperty("errorMessage").ToString();
    if (!string.IsNullOrEmpty(errorMessage))
    {
      _logger.LogInformation("Error message: {0}", errorMessage);
      return Enumerable.Empty<MovieJsonShort>();
    }
    string jsonDoc = JsonDocument.Parse(httpResponse).RootElement.GetProperty("results").ToString();
    return JsonSerializer.Deserialize<IEnumerable<MovieJsonShort>>(jsonDoc) ?? Enumerable.Empty<MovieJsonShort>();
  }

  private MovieJson? Deserialize(string httpResponse)
  {
    string errorMessage = JsonDocument.Parse(httpResponse).RootElement.GetProperty("errorMessage").ToString();
    if (!string.IsNullOrEmpty(errorMessage))
    {
      _logger.LogInformation("Error message: {0}", errorMessage);
      return new MovieJson() { ErrorMessage = errorMessage };
    }
    string jsonDoc = JsonDocument.Parse(httpResponse).RootElement.ToString();
    return JsonSerializer.Deserialize<MovieJson>(jsonDoc);
  }
}
