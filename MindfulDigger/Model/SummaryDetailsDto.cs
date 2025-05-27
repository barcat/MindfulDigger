using System;

namespace MindfulDigger.Model
{
    public class SummaryDetailsDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Content { get; set; }
        public DateTimeOffset GenerationDate { get; set; }
        public string? PeriodDescription { get; set; }
        public DateTimeOffset? PeriodStart { get; set; }
        public DateTimeOffset? PeriodEnd { get; set; }
        public bool IsAutomatic { get; set; }
        public DateTime CreatedAt { get; set; } // Added
        public string? Status { get; set; }      // Added
    }
}