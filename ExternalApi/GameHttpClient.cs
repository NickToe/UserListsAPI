using System.Text.Json;
using UserListsAPI.JsonModels;

namespace UserListsAPI.ExternalApi;

public class GameHttpClient : ItemHttpClientBase
{
    private readonly ILogger<GameHttpClient> _logger;
    private readonly HttpClient _client = new();

    public GameHttpClient(ILogger<GameHttpClient> logger, IConfiguration configuration) : base(logger, configuration, "Game")
    {
        _logger = logger;
    }

    public async Task<GameJson?> GetByIdAsync(string id)
    {
        string requestUrl = $"https://store.steampowered.com/api/appdetails?appids={id}&l=english";
        string? httpResponseBody = await SendApiRequestAsync(requestUrl);
        if (string.IsNullOrEmpty(httpResponseBody)) return default;
        return Deserialize(id, httpResponseBody);
    }

    public async Task<GameReviewsJson?> GetReviewsByIdAsync(string id)
    {
        string requestUrl = $"https://store.steampowered.com/appreviews/{id}/?json=1&language=all&purchase_type=all&num_per_page=0";
        string? httpResponseBody = await SendApiRequestAsync(requestUrl);
        if (string.IsNullOrEmpty(httpResponseBody)) return default;
        return DeserializeReviews(httpResponseBody);
    }

    /// <summary>
    /// Get list of deserialized games from Steam API as {id}-{title}
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<GameJsonShort>> GetListAsync()
    {
        int lastAppid = 0;
        bool moreResults = true;
        string requestUrl = string.Empty;
        List<GameJsonShort> gamesJsonShort = new List<GameJsonShort>();
        while (moreResults)
        {
            requestUrl = $"{_apiUrl}/IStoreService/GetAppList/v1/?max_results=50000&key={_apiKey}&last_appid={lastAppid}";
            string? httpResponseBody = await SendApiRequestAsync(requestUrl);
            if (string.IsNullOrEmpty(httpResponseBody)) continue;
            gamesJsonShort.AddRange(DeserializeList(httpResponseBody, out moreResults, out lastAppid));
        }
        return gamesJsonShort;
    }

    private GameReviewsJson? DeserializeReviews(string httpResponseBody)
    {
        string jsonDoc = JsonDocument.Parse(httpResponseBody).RootElement.GetProperty("query_summary").ToString();
        return JsonSerializer.Deserialize<GameReviewsJson>(jsonDoc);
    }

    private GameJson? Deserialize(string id, string httpResponseBody)
    {
        JsonElement jsonElement = JsonDocument.Parse(httpResponseBody).RootElement.GetProperty(id);
        bool success = jsonElement.GetProperty("success").GetBoolean();
        if (!success) return default;
        string jsonDoc = JsonDocument.Parse(httpResponseBody).RootElement.GetProperty(id).GetProperty("data").ToString();
        return JsonSerializer.Deserialize<GameJson>(jsonDoc);
    }

    private IEnumerable<GameJsonShort> DeserializeList(string httpResponseBody, out bool moreResults, out int lastAppid)
    {
        JsonElement jsonElement = JsonDocument.Parse(httpResponseBody).RootElement.GetProperty("response");
        if (jsonElement.TryGetProperty("have_more_results", out JsonElement moreResElement))
        {
            moreResults = moreResElement.GetBoolean();
            lastAppid = jsonElement.GetProperty("last_appid").GetInt32();
        }
        else
        {
            moreResults = false;
            lastAppid = 0;
        }
        string jsonDoc = jsonElement.GetProperty("apps").ToString();
        return JsonSerializer.Deserialize<IEnumerable<GameJsonShort>>(jsonDoc) ?? Enumerable.Empty<GameJsonShort>();
    }
}