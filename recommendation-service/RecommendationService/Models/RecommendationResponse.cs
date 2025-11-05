namespace RecommendationService.Models;

public class RecommendationResponse
{
    public string Reply { get; set; } = string.Empty;
    public bool Success { get; set; } = true;
    public string? Error { get; set; }
}

