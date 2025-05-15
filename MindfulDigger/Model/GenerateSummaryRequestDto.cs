// Filepath: d:\git\MindfulDigger\MindfulDigger\DTOs\GenerateSummaryRequestDto.cs
using System.ComponentModel.DataAnnotations;
using MindfulDigger.Attributes; // Add this using directive

namespace MindfulDigger.DTOs
{
    public class GenerateSummaryRequestDto
    {
        [Required(ErrorMessage = "Period is required.")]
        [ValidSummaryPeriod] // Add this attribute
        public string? Period { get; set; }
    }
}
