using System.Text.Json.Serialization;

namespace UserListsAPI.JsonModels;

public record GameReviewsJson
{
  [JsonPropertyName("review_score_desc")]
  public string ReviewScore { get; set; } = null!;
  [JsonPropertyName("total_positive")]
  public int TotalPositive { get; set; }
  [JsonPropertyName("total_negative")]
  public int TotalNegative { get; set; }
  [JsonPropertyName("total_reviews")]
  public int TotalReviews { get; set; }
}