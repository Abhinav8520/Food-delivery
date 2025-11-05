namespace RecommendationService.Models;

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool Availability { get; set; }
    public string SpiceLevel { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public List<string> Tags { get; set; } = new();
}

