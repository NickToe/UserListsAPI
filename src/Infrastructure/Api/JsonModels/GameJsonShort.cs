using System.Text.Json.Serialization;

namespace Infrastructure.Api.JsonModels;

public record GameJsonShort
{
    [JsonPropertyName("appid")]
    [JsonConverter(typeof(StringJsonConverter))]
    public string Id { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Title { get; set; } = null!;
}
