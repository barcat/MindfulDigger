using Microsoft.AspNetCore.Http;
using MindfulDigger.Commands;
using MindfulDigger.DTOs;
using MindfulDigger.Models;
using Microsoft.Extensions.Logging;
using Supabase;
using Supabase.Interfaces;

namespace MindfulDigger.Services;

public interface IFeedbackService
{
    Task<(FeedbackResponseDto Dto, int StatusCode)> CreateOrUpdateFeedbackAsync(CreateOrUpdateFeedbackCommand command);
}

public class FeedbackService : IFeedbackService
{    private readonly ISqlClientFactory _supabaseClientFactory;
    private readonly ISummaryService _summaryService;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(
        ISqlClientFactory supabaseClientFactory,
        ISummaryService summaryService, 
        ILogger<FeedbackService> logger)
    {
        _supabaseClientFactory = supabaseClientFactory ?? throw new ArgumentNullException(nameof(supabaseClientFactory));
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
        }        try
        {
            var supabase = await _supabaseClientFactory.CreateClient();
            var feedback = new Feedback
            {
                SummaryId = command.SummaryId.ToString(),
                UserId = command.UserId.ToString(),
                Rating = command.Rating,
                CreationDate = DateTime.UtcNow
            };

            var response = await supabase.From<Feedback>().Upsert(feedback);
            var updatedFeedback = response.Models.FirstOrDefault();

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

            var dto = new FeedbackResponseDto
            {
                SummaryId = Guid.Parse(updatedFeedback.SummaryId),
                UserId = Guid.Parse(updatedFeedback.UserId),
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
