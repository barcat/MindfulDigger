namespace MindfulDigger.Model;

public record CreateOrUpdateFeedbackCommand(
    Guid SummaryId, 
    Guid UserId, 
    FeedbackRating Rating,
    string Jwt,
    string RefreshToken
);
