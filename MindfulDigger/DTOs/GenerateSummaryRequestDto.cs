// Filepath: d:\git\MindfulDigger\MindfulDigger\DTOs\GenerateSummaryRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace MindfulDigger.DTOs
{
    public class GenerateSummaryRequestDto
    {
        [Required(ErrorMessage = "Period is required.")]
        // TODO: Add custom validation attribute or regex for specific period values
        // e.g., [RegularExpression("^(last_7_days|last_14_days|last_30_days|last_10_notes)$", ErrorMessage = "Invalid period value.")]
        public string Period { get; set; }

        public GenerateSummaryRequestDto()
        {
            Period = string.Empty; // Initialize to satisfy non-nullable warnings if any
        }
    }
}
