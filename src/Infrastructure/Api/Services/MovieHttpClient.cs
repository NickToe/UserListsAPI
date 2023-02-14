using System.Text.Json;
using Infrastructure.Api.Abstractions;
using Infrastructure.Api.JsonModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Api.Services;

public class MovieHttpClient : ItemHttpClientBase
{
    private readonly ILogger<MovieHttpClient> _logger;

    public MovieHttpClient(ILogger<MovieHttpClient> logger, IConfiguration configuration) : base(logger, configuration, "Movie")
    {
        _logger = logger;
    }

    public async Task<MovieJson?> GetByIdAsync(string id)
    {
        string requestUrl = $"{_apiUrl}/Title/{_apiKey}/{id}";
        string? httpResponseBody = await SendApiRequestAsync(requestUrl);
        if (string.IsNullOrEmpty(httpResponseBody)) return default;
        return Deserialize(httpResponseBody);
    }

    public async Task<IEnumerable<MovieJsonShort>> GetAllByTitleAsync(string title)
    {
        string requestUrl = $"{_apiUrl}/SearchMovie/{_apiKey}/{title}";
        string? httpResponseBody = await SendApiRequestAsync(requestUrl);
        if (string.IsNullOrEmpty(httpResponseBody)) return Enumerable.Empty<MovieJsonShort>();
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
