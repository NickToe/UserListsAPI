using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using UserListsAPI.Data.Enums;

namespace UserListsAPI.Data.Entities;

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
    public ItemStatus ItemStatus { get; set; } = ItemStatus.Ok;

    public bool IsFilled() => Poster != default && Type != default && ShortDescription != default;
}