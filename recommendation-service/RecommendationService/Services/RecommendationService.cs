using RecommendationService.Models;

namespace RecommendationService.Services;

public interface IRecommendationService
{
    Task<RecommendationResponse> GenerateRecommendationAsync(RecommendationRequest request);
}

public class RecommendationService : IRecommendationService
{
    private readonly IMenuService _menuService;
    private readonly IGptService _gptService;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(
        IMenuService menuService,
        IGptService gptService,
        ILogger<RecommendationService> logger)
    {
        _menuService = menuService;
        _gptService = gptService;
        _logger = logger;
    }

    public async Task<RecommendationResponse> GenerateRecommendationAsync(RecommendationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return new RecommendationResponse
                {
                    Success = false,
                    Error = "Message is required"
                };
            }

            _logger.LogInformation($"Generating recommendation for session {request.SessionId}");

            // Get available menu items
            var menuItems = _menuService.GetAvailableItems();
            
            if (!menuItems.Any())
            {
                return new RecommendationResponse
                {
                    Reply = "I'm sorry, but we don't have any available items at the moment. Please check back later!",
                    Success = true
                };
            }

            // Generate recommendation using GPT-3.5 Turbo
            var recommendation = await _gptService.GenerateRecommendationAsync(
                request.Message,
                menuItems,
                request.ConversationHistory
            );

            return new RecommendationResponse
            {
                Reply = recommendation,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendation");
            return new RecommendationResponse
            {
                Success = false,
                Error = "An error occurred while generating recommendations. Please try again later.",
                Reply = "I'm sorry, I encountered an error while processing your request. Please try again!"
            };
        }
    }
}

