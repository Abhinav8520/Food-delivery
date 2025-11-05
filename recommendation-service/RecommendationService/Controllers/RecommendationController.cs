using Microsoft.AspNetCore.Mvc;
using RecommendationService.Models;
using RecommendationService.Services;

namespace RecommendationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly ILogger<RecommendationController> _logger;

    public RecommendationController(
        IRecommendationService recommendationService,
        ILogger<RecommendationController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<RecommendationResponse>> GenerateRecommendation([FromBody] RecommendationRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new RecommendationResponse
                {
                    Success = false,
                    Error = "Message is required"
                });
            }

            _logger.LogInformation($"Received recommendation request for session: {request.SessionId}");

            var response = await _recommendationService.GenerateRecommendationAsync(request);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GenerateRecommendation endpoint");
            return StatusCode(500, new RecommendationResponse
            {
                Success = false,
                Error = "Internal server error",
                Reply = "I'm sorry, something went wrong. Please try again later."
            });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "recommendation-service" });
    }
}

