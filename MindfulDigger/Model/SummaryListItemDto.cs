namespace MindfulDigger.Model
{
    public class SummaryListItemDto
    {
        public Guid Id { get; set; }
        public DateTime GenerationDate { get; set; }
        public string? PeriodDescription { get; set; }
        public bool IsAutomatic { get; set; }
    }
}