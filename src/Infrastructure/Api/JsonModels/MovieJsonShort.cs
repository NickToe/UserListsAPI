﻿using System.Text.Json.Serialization;

namespace Infrastructure.Api.JsonModels;

public record MovieJsonShort
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("image")]
    public string Poster { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    public bool IsValid() => !string.IsNullOrEmpty(Poster) && !string.IsNullOrEmpty(Description);
}