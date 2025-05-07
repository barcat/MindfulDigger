using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindfulDigger.DTOs;
using MindfulDigger.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MindfulDigger.Models; // Required for Summary model

namespace MindfulDigger.Controllers
{
    [Authorize]
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
    }
}
