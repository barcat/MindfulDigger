namespace MindfulDigger.Model;

public class FeedbackResponseDto
{
    public required Guid SummaryId { get; set; }
    public required Guid UserId { get; set; }
    public required FeedbackRating Rating { get; set; }
    public required DateTime CreationDate { get; set; }
}
