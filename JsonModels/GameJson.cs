using System.Text.Json;
using System.Text.Json.Serialization;

namespace UserListsAPI.JsonModels;

public record GameJson
{
  [JsonPropertyName("steam_appid")]
  [JsonConverter(typeof(StringJsonConverter))]
  public string Id { get; set; } = null!;

  [JsonPropertyName("name")]
  public string Title { get; set; } = null!;

  [JsonPropertyName("type")]
  public string Type { get; set; } = null!;

  [JsonPropertyName("header_image")]
  public string Poster { get; set; } = null!;

  [JsonPropertyName("short_description")]
  public string ShortDescription { get; set; } = null!;

  [JsonPropertyName("genres")]
  public List<GameJsonGenre>? Genres { get; set; }

  [JsonPropertyName("developers")]
  public List<string> Developers { get; set; } = null!;

  [JsonPropertyName("publishers")]
  public List<string>? Publishers { get; set; }

  [JsonPropertyName("release_date")]
  public ReleaseDate ReleaseDate { get; set; } = null!;

  [JsonPropertyName("metacritic")]
  public Metacritic? Metacritic { get; set; }
}

public record ReleaseDate
{
  [JsonPropertyName("coming_soon")]
  public bool? ComingSoon { get; set; }

  [JsonPropertyName("date")]
  public string? Date { get; set; }
}

public record GameJsonGenre
{
  [JsonPropertyName("id")]
  public string GenreId { get; set; } = null!;

  [JsonPropertyName("description")]
  public string Description { get; set; } = null!;
}

public record Metacritic
{
  [JsonPropertyName("score")]
  public short? Score { get; set; }

  [JsonPropertyName("url")]
  public string? Url { get; set; }
}

internal class StringJsonConverter : JsonConverter<string>
{
  public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetInt32().ToString();
  public override void Write(Utf8JsonWriter writer, string id, JsonSerializerOptions options) => writer.WriteNumberValue(Int32.Parse(id));
}