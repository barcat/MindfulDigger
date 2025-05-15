using Microsoft.AspNetCore.Mvc;
using MindfulDigger.DTOs;
using MindfulDigger.Services;
using System.Security.Claims;


namespace MindfulDigger.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SummariesController : ControllerBase
    {
        private readonly ISummaryService _summaryService;
        private readonly ILogger<SummariesController> _logger;

        public SummariesController(ISummaryService summaryService, ILogger<SummariesController> logger)
        {
            _summaryService = summaryService ?? throw new ArgumentNullException(nameof(summaryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{summaryId:guid}")]
        public async Task<IActionResult> GetSummaryById(Guid summaryId)
        {
            if (summaryId == Guid.Empty)
            {
                _logger.LogWarning("GetSummaryById called with empty summaryId.");
                return BadRequest("Summary ID cannot be empty."); // Or NotFound as per preference
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                _logger.LogWarning("User ID not found in token or is invalid.");
                return Unauthorized(); // Or Forbid() if user is authenticated but claim is missing
            }

            try
            {
                var summary = await _summaryService.GetSummaryByIdAsync(summaryId);

                if (summary == null)
                {
                    _logger.LogInformation("Summary with ID {SummaryId} not found for GetSummaryById.", summaryId);
                    return NotFound();
                }

                if (summary.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to access summary {SummaryId} owned by {OwnerUserId}. Access denied.", userId, summaryId, summary.UserId);
                    return Forbid(); // User is authenticated but not authorized for this resource
                }

                var summaryDto = new SummaryDetailsDto
                {
                    Id = summary.Id,
                    UserId = summary.UserId,
                    Content = summary.Content,
                    GenerationDate = summary.GenerationDate,
                    PeriodDescription = summary.PeriodDescription,
                    PeriodStart = summary.PeriodStart,
                    PeriodEnd = summary.PeriodEnd,
                    IsAutomatic = summary.IsAutomatic
                };
                _logger.LogInformation("Successfully retrieved summary {SummaryId} for user {UserId}.", summaryId, userId);
                return Ok(summaryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving summary {SummaryId} for user {UserId}.", summaryId, userId);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSummaries([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Attempting to get summaries for user. Page: {Page}, PageSize: {PageSize}", page, pageSize);

            if (page < 1)
            {
                _logger.LogWarning("Invalid page number requested: {Page}", page);
                return BadRequest(new { error = "Page number must be 1 or greater." });
            }
            
            // Define a max page size, e.g., 100, as per implementation plan
            const int maxPageSize = 100;
            if (pageSize < 1 || pageSize > maxPageSize)
            {
                _logger.LogWarning("Invalid page size requested: {PageSize}. Must be between 1 and {MaxPageSize}.", pageSize, maxPageSize);
                return BadRequest(new { error = $"Page size must be between 1 and {maxPageSize}." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // This case should ideally be handled by the [Authorize] attribute,
                // but as a safeguard:
                _logger.LogWarning("User ID not found in token for GetSummaries.");
                return Unauthorized(new { error = "User ID not found in token." });
            }

            try
            {
                var paginatedResult = await _summaryService.GetSummariesAsync(userId, page, pageSize);
                
                // The plan's response structure is slightly different from PaginatedResponse<T>
                // It expects "summaries" and "pagination" top-level keys.
                var response = new
                {
                    summaries = paginatedResult.Items,
                    pagination = paginatedResult.Pagination
                };
                
                _logger.LogInformation("Successfully retrieved summaries for user {UserId}. Page: {Page}, PageSize: {PageSize}, TotalCount: {TotalCount}", userId, page, pageSize, paginatedResult.Pagination.TotalCount);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving summaries for user {UserId}. Page: {Page}, PageSize: {PageSize}", userId, page, pageSize);
                // As per plan, return a generic 500 error message
                return StatusCode(500, new { error = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSummary([FromBody] GenerateSummaryRequestDto requestDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token for GenerateSummary.");
                return Unauthorized(new { message = "User ID not found in token." }); // Return a ProblemDetails-like object or simple message
            }

            _logger.LogInformation("GenerateSummary called by user {UserId} with period {Period}", userId, requestDto.Period);

            var (summaryDto, statusCode) = await _summaryService.GenerateSummaryAsync(userId, requestDto);

            return statusCode switch
            {
                StatusCodes.Status201Created => CreatedAtAction(nameof(GetSummaryById), new { summaryId = summaryDto.Id }, summaryDto),
                StatusCodes.Status400BadRequest => BadRequest(new { title = "Validation Error", status = StatusCodes.Status400BadRequest, detail = summaryDto.Content }), // Assuming content holds error message
                StatusCodes.Status500InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, new { title = "Internal Server Error", status = StatusCodes.Status500InternalServerError, detail = summaryDto.Content }),
                _ => StatusCode(statusCode, summaryDto) 
            };
        }
    }
}
