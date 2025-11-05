using System.Text;
using System.Text.Json;
using RecommendationService.Models;

namespace RecommendationService.Services;

public interface IGptService
{
    Task<string> GenerateRecommendationAsync(string userMessage, List<MenuItem> menuItems, List<ConversationMessage>? conversationHistory);
}

public class GptService : IGptService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GptService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GptService(IConfiguration configuration, ILogger<GptService> logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        
        // Load from environment variable (which should be loaded from backend/.env)
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
                  ?? _configuration["OpenAI:ApiKey"] 
                  ?? _configuration["OPENAI_API_KEY"] 
                  ?? "";
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
    }

    public async Task<string> GenerateRecommendationAsync(
        string userMessage, 
        List<MenuItem> menuItems, 
        List<ConversationMessage>? conversationHistory)
    {
        try
        {
            // Build the system prompt with menu context
            var menuContext = BuildMenuContext(menuItems);
            var systemPrompt = $@"You are Ziggy, a friendly food recommendation assistant for a food delivery service. 
Your role is to help customers find the perfect food items from our menu.

Here is our current menu:
{menuContext}

When making recommendations:
1. Consider the user's preferences, dietary restrictions, spice level tolerance, and budget
2. Only recommend items that are currently available (availability: true)
3. Consider ratings and popular choices
4. Provide personalized explanations for why you're recommending each item
5. Format recommendations clearly with item names, descriptions, prices, and ratings
6. Be conversational and friendly
7. If the user's request doesn't match any items well, suggest the closest alternatives with explanations";

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Add conversation history if available
            if (conversationHistory != null && conversationHistory.Any())
            {
                foreach (var msg in conversationHistory)
                {
                    messages.Add(new { role = msg.Role.ToLower(), content = msg.Content });
                }
            }

            // Add current user message
            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = messages,
                temperature = 0.7,
                max_tokens = 500
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseContent);

            var recommendation = responseJson.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "I'm sorry, I couldn't generate a recommendation at this time.";

            _logger.LogInformation("Successfully generated recommendation from GPT-3.5 Turbo");
            return recommendation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendation from GPT-3.5 Turbo");
            throw;
        }
    }

    private string BuildMenuContext(List<MenuItem> menuItems)
    {
        var contextBuilder = new System.Text.StringBuilder();
        var availableItems = menuItems.Where(item => item.Availability).ToList();

        foreach (var item in availableItems)
        {
            contextBuilder.AppendLine($"- {item.Name} (ID: {item.Id})");
            contextBuilder.AppendLine($"  Description: {item.Description}");
            contextBuilder.AppendLine($"  Category: {item.Category}");
            contextBuilder.AppendLine($"  Price: ${item.Price}");
            contextBuilder.AppendLine($"  Rating: {item.Rating}/5");
            contextBuilder.AppendLine($"  Spice Level: {item.SpiceLevel}");
            contextBuilder.AppendLine($"  Tags: {string.Join(", ", item.Tags)}");
            contextBuilder.AppendLine();
        }

        return contextBuilder.ToString();
    }
}

