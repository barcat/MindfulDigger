using System;
using System.Threading.Tasks;
using MindfulDigger.Models;
using MindfulDigger.DTOs;

namespace MindfulDigger.Services
{
    public interface ISummaryService
    {
        Task<Summary?> GetSummaryByIdAsync(Guid summaryId);
        Task<PaginatedResponse<SummaryListItemDto>> GetSummariesAsync(string userId, int page, int pageSize);
        Task<(object Dto, int StatusCode)> RequestSummaryGenerationAsync(string userId, GenerateSummaryRequestDto requestDto);
    }
}