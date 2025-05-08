// Filepath: d:\git\MindfulDigger\MindfulDigger\DTOs\GenerateSummaryAcceptedResponseDto.cs
namespace MindfulDigger.DTOs
{
    public class GenerateSummaryAcceptedResponseDto
    {
        public string Message { get; set; }
        public string StatusCheckUrl { get; set; } // e.g., /api/summaries/generation/status/{jobId}

        public GenerateSummaryAcceptedResponseDto()
        {
            Message = string.Empty;
            StatusCheckUrl = string.Empty;
        }
    }
}
