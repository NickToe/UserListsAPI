namespace Application.DTOs;

public class GameDTO
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public List<string> Developers { get; set; } = null!;
    public List<string> Genres { get; set; } = null!;
    public string Poster { get; set; } = null!;
    public short MetacriticScore { get; set; }
    public string MetacriticUrl { get; set; } = null!;
    public List<string> Publishers { get; set; } = null!;
    public bool ComingSoon { get; set; }
    public string ReleaseDate { get; set; } = null!;
    public string ShortDescription { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string ReviewScore { get; set; } = null!;
    public int TotalPositive { get; set; }
    public int TotalNegative { get; set; }
    public int TotalReviews { get; set; }
}
