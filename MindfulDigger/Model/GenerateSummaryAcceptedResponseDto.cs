namespace MindfulDigger.Model
{
    public class GenerateSummaryAcceptedResponseDto
    {
        public string Message { get; set; }
        public string StatusCheckUrl { get; set; }

        public GenerateSummaryAcceptedResponseDto()
        {
            Message = string.Empty;
            StatusCheckUrl = string.Empty;
        }
    }
}
