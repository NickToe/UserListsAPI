using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using UserListsAPI.DataLayer.Enums;
using UserListsAPI.JsonModels;

namespace UserListsAPI.DataLayer.Entities;

[Index(nameof(Id))]
[Index(nameof(Title))]
public record Movie
{
  public Movie(string id, string title, string poster)
  {
    Id = id;
    Title = title;
    Poster = poster;
  }

  [Key]
  public string Id { get; set; } = null!;
  [Required]
  public string Title { get; set; } = null!;
  public string? FullTitle { get; set; }
  public string? Type { get; set; }
  public string? Year { get; set; }
  public string Poster { get; set; } = null!;
  public string? ReleaseDate { get; set; }
  public string? RuntimeMins { get; set; }
  public string? RuntimeStr { get; set; }
  public string? Plot { get; set; }
  public string? Directors { get; set; }
  public string? Stars { get; set; }
  public string? Genres { get; set; }
  public string? Companies { get; set; }
  public string? Countries { get; set; }
  public string? ContentRating { get; set; }
  public string? ImdbRating { get; set; }
  public string? ImdbRatingVotes { get; set; }
  public string? MetascriticRating { get; set; }
  [JsonIgnore]
  public ItemStatus ItemStatus { get; set; } = ItemStatus.Ok;

  public static string GetFullTitle(string title, string year) => $"{title} ({year})";

  public static Movie ToEntity(MovieJson movieJson) => new(movieJson.Id, movieJson.Title, movieJson.Poster)
  {
    FullTitle = movieJson.FullTitle,
    Type = movieJson.Type,
    Year = movieJson.Year,
    ReleaseDate = movieJson.ReleaseDate,
    RuntimeMins = movieJson.RuntimeMins,
    RuntimeStr = movieJson.RuntimeStr,
    Plot = movieJson.Plot,
    Directors = movieJson.Directors,
    Stars = movieJson.Stars,
    Genres = movieJson.Genres,
    Companies = movieJson.Companies,
    Countries = movieJson.Countries,
    ContentRating = movieJson.ContentRating,
    ImdbRating = movieJson.ImdbRating,
    ImdbRatingVotes = movieJson.ImdbRatingVotes,
    MetascriticRating = movieJson.MetascriticRating
  };
}