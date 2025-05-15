using MindfulDigger.Data.Supabase;
using MindfulDigger.DTOs;
using MindfulDigger.Model;

namespace MindfulDigger.Services;

public interface IFeedbackService
{
    Task<(FeedbackResponseDto Dto, int StatusCode)> CreateOrUpdateFeedbackAsync(CreateOrUpdateFeedbackCommand command);
}

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly ISummaryService _summaryService;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(
        IFeedbackRepository feedbackRepository,
        ISummaryService summaryService,
        ILogger<FeedbackService> logger)
    {
        _feedbackRepository = feedbackRepository ?? throw new ArgumentNullException(nameof(feedbackRepository));
        _summaryService = summaryService ?? throw new ArgumentNullException(nameof(summaryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(FeedbackResponseDto Dto, int StatusCode)> CreateOrUpdateFeedbackAsync(CreateOrUpdateFeedbackCommand command)
    {
        _logger.LogInformation("Creating/updating feedback for summary {SummaryId} by user {UserId}", command.SummaryId, command.UserId);

        // Check if summary exists and belongs to user
        var summary = await _summaryService.GetSummaryByIdAsync(command.SummaryId);
        if (summary == null)
        {
            _logger.LogWarning("Summary {SummaryId} not found", command.SummaryId);
            return (new FeedbackResponseDto
            {
                SummaryId = command.SummaryId,
                UserId = command.UserId,
                Rating = command.Rating,
                CreationDate = DateTime.UtcNow
            }, StatusCodes.Status404NotFound);
        }

        if (summary.UserId.ToString() != command.UserId.ToString())
        {
            _logger.LogWarning("User {UserId} attempted to give feedback on summary {SummaryId} owned by {OwnerUserId}",
                command.UserId, command.SummaryId, summary.UserId);
            return (new FeedbackResponseDto
            {
                SummaryId = command.SummaryId,
                UserId = command.UserId,
                Rating = command.Rating,
                CreationDate = DateTime.UtcNow
            }, StatusCodes.Status403Forbidden);
        }

        try
        {
            var feedback = new Feedback
            {
                SummaryId = command.SummaryId.ToString(),
                UserId = command.UserId.ToString(),
                Rating = command.Rating,
                CreationDate = DateTime.UtcNow
            };

            var updatedFeedback = await _feedbackRepository.UpsertFeedbackAsync(feedback);

            if (updatedFeedback == null)
            {
                _logger.LogError("Failed to create/update feedback for summary {SummaryId} by user {UserId}", command.SummaryId, command.UserId);
                return (new FeedbackResponseDto
                {
                    SummaryId = command.SummaryId,
                    UserId = command.UserId,
                    Rating = command.Rating,
                    CreationDate = DateTime.UtcNow
                }, StatusCodes.Status500InternalServerError);
            }

            if (string.IsNullOrEmpty(updatedFeedback.SummaryId) || string.IsNullOrEmpty(updatedFeedback.UserId))
            {
                _logger.LogError("Updated feedback contains null or empty SummaryId or UserId. SummaryId: '{SummaryId}', UserId: '{UserId}'", updatedFeedback.SummaryId, updatedFeedback.UserId);
                return (new FeedbackResponseDto
                {
                    SummaryId = command.SummaryId,
                    UserId = command.UserId,
                    Rating = command.Rating,
                    CreationDate = DateTime.UtcNow
                }, StatusCodes.Status500InternalServerError);
            }

            Guid parsedSummaryId, parsedUserId;
            bool summaryIdParsed = Guid.TryParse(updatedFeedback.SummaryId, out parsedSummaryId);
            bool userIdParsed = Guid.TryParse(updatedFeedback.UserId, out parsedUserId);

            if (!summaryIdParsed || !userIdParsed)
            {
                _logger.LogError("Failed to parse Guid from updated feedback. SummaryId string: '{SummaryId}', UserId string: '{UserId}'", updatedFeedback.SummaryId, updatedFeedback.UserId);
                return (new FeedbackResponseDto
                {
                    SummaryId = command.SummaryId,
                    UserId = command.UserId,
                    Rating = command.Rating,
                    CreationDate = DateTime.UtcNow
                }, StatusCodes.Status500InternalServerError);
            }

            var dto = new FeedbackResponseDto
            {
                SummaryId = parsedSummaryId,
                UserId = parsedUserId,
                Rating = updatedFeedback.Rating,
                CreationDate = updatedFeedback.CreationDate
            };

            return (dto, StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating feedback for summary {SummaryId} by user {UserId}", command.SummaryId, command.UserId);
            return (new FeedbackResponseDto
            {
                SummaryId = command.SummaryId,
                UserId = command.UserId,
                Rating = command.Rating,
                CreationDate = DateTime.UtcNow
            }, StatusCodes.Status500InternalServerError);
        }
    }
}
