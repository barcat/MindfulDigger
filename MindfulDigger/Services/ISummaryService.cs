using MindfulDigger.Model;

namespace MindfulDigger.Services
{
    public interface ISummaryService
    {
        Task<Summary?> GetSummaryByIdAsync(Guid summaryId, string jwt, string refreshToken);
        Task<PaginatedResponse<SummaryListItemDto>> GetSummariesAsync(string userId, int page, int pageSize, string jwt, string refreshToken);
        Task<(SummaryDetailsDto Dto, int StatusCode)> GenerateSummaryAsync(string userId, GenerateSummaryRequestDto requestDto, string jwt, string refreshToken);
    }
}