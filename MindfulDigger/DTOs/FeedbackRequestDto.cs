using System.ComponentModel.DataAnnotations;
using MindfulDigger.Models;

namespace MindfulDigger.DTOs;

public class FeedbackRequestDto
{
    [Required]
    public Guid SummaryId { get; set; }

    [Required]
    [EnumDataType(typeof(FeedbackRating))]
    public FeedbackRating Rating { get; set; }
}
