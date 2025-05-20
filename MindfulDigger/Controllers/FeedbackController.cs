using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindfulDigger.Model;
using MindfulDigger.Services;
using System.Security.Claims;

namespace MindfulDigger.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly ILogger<FeedbackController> _logger;

    public FeedbackController(
        IFeedbackService feedbackService,
        ILogger<FeedbackController> logger)
    {
        _feedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ProducesResponseType(typeof(FeedbackResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FeedbackResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrUpdateFeedback([FromBody] FeedbackRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in token.");
            return Unauthorized(new { message = "User ID not found in token." });
        }

        var jwt = User.FindFirstValue("AccessToken") ?? string.Empty;
        var refreshToken = User.FindFirstValue("RefreshToken") ?? string.Empty;

        var command = new CreateOrUpdateFeedbackCommand(
            request.SummaryId,
            Guid.Parse(userId),
            request.Rating,
            jwt,
            refreshToken
        );

        var (response, statusCode) = await _feedbackService.CreateOrUpdateFeedbackAsync(command);

        return statusCode switch
        {
            StatusCodes.Status200OK => Ok(response),
            StatusCodes.Status403Forbidden => Forbid(),
            StatusCodes.Status404NotFound => NotFound(response),
            StatusCodes.Status500InternalServerError => StatusCode(500, response),
            _ => StatusCode(statusCode, response)
        };
    }
}
