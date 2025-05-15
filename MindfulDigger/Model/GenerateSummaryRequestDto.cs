using System.ComponentModel.DataAnnotations;
using MindfulDigger.Attributes; 

namespace MindfulDigger.Model
{
    public class GenerateSummaryRequestDto
    {
        [Required(ErrorMessage = "Period is required.")]
        [ValidSummaryPeriod] 
        public string? Period { get; set; }
    }
}
