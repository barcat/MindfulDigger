using System;

namespace MindfulDigger.DTOs
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
    }
}