namespace UserListsAPI.DTOs;

public class MovieDTO
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string FullTitle { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Year { get; set; } = null!;
    public string Poster { get; set; } = null!;
    public string ReleaseDate { get; set; } = null!;
    public string RuntimeMins { get; set; } = null!;
    public string RuntimeStr { get; set; } = null!;
    public string Plot { get; set; } = null!;
    public string Directors { get; set; } = null!;
    public string Stars { get; set; } = null!;
    public string Genres { get; set; } = null!;
    public string Companies { get; set; } = null!;
    public string Countries { get; set; } = null!;
    public string ContentRating { get; set; } = null!;
    public string ImdbRating { get; set; } = null!;
    public string ImdbRatingVotes { get; set; } = null!;
    public string MetascriticRating { get; set; } = null!;
}
