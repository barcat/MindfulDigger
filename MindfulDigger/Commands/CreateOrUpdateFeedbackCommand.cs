using MindfulDigger.Models;

namespace MindfulDigger.Commands;

public record CreateOrUpdateFeedbackCommand(
    Guid SummaryId, 
    Guid UserId, 
    FeedbackRating Rating
);
