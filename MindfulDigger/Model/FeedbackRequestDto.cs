using System.ComponentModel.DataAnnotations;
using MindfulDigger.Model;

namespace MindfulDigger.Model;

public class FeedbackRequestDto
{
    [Required]
    public Guid SummaryId { get; set; }

    [Required]
    [EnumDataType(typeof(FeedbackRating))]
    public FeedbackRating Rating { get; set; }
}
