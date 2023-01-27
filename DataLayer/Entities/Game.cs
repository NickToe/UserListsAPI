using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using UserListsAPI.DataLayer.Enums;
using UserListsAPI.JsonModels;

namespace UserListsAPI.DataLayer.Entities;

[Index(nameof(Id))]
[Index(nameof(Title))]
public record Game
{
  public Game(string id, string title)
  {
    Id = id;
    Title = title;
  }

  [Key]
  public string Id { get; set; } = null!;
  [Required]
  public string Title { get; set; } = null!;
  public List<string>? Developers { get; set; }
  public List<string>? Genres { get; set; }
  public string? Poster { get; set; }
  public short? MetacriticScore { get; set; }
  public string? MetacriticUrl { get; set; }
  public List<string>? Publishers { get; set; }
  public bool? ComingSoon { get; set; }
  public string? ReleaseDate { get; set; }
  public string? ShortDescription { get; set; }
  public string? Type { get; set; }
  public string? ReviewScore { get; set; }
  public int? TotalPositive { get; set; }
  public int? TotalNegative { get; set; }
  public int? TotalReviews { get; set; }
  [JsonIgnore]
  public ItemStatus ItemStatus { get; set; } = ItemStatus.Ok;

  public bool IsFilled() => (Poster != default && Type != default && ShortDescription != default);

  public static Game ToEntity(GameJson gameJson, GameReviewsJson? gameReviewsJson) => new(gameJson.Id, gameJson.Title)
  {
    Developers = gameJson?.Developers,
    Publishers = gameJson?.Publishers,
    ShortDescription = gameJson?.ShortDescription,
    Poster = gameJson?.Poster,
    Genres = gameJson?.Genres?.Select(genre => genre.Description).ToList(),
    MetacriticScore = gameJson?.Metacritic?.Score,
    MetacriticUrl = gameJson?.Metacritic?.Url,
    ComingSoon = gameJson?.ReleaseDate?.ComingSoon,
    ReleaseDate = gameJson?.ReleaseDate?.Date,
    Type = gameJson?.Type,
    ReviewScore = gameReviewsJson?.ReviewScore,
    TotalPositive = gameReviewsJson?.TotalPositive,
    TotalNegative = gameReviewsJson?.TotalNegative,
    TotalReviews = gameReviewsJson?.TotalReviews,
  };
}