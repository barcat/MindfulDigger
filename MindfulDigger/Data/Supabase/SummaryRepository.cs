using MindfulDigger.Models;
using Supabase;
using MindfulDigger.Services;
using Supabase.Postgrest;

namespace MindfulDigger.Data.Supabase;

public interface ISummaryRepository
{
    Task<Summary?> GetSummaryByIdAsync(Guid summaryId);
    Task<(List<Summary> Summaries, long TotalCount)> GetUserSummariesPaginatedAsync(Guid userId, int page, int pageSize);
    Task<Summary?> InsertSummaryAsync(Summary summary);
    Task<List<Note>> GetNotesForSummaryAsync(Guid userId, string period, DateTimeOffset? periodStart = null, DateTimeOffset? periodEnd = null);
}

public class SummaryRepository : ISummaryRepository
{
    private readonly ISqlClientFactory _supabaseClientFactory;

    public SummaryRepository(ISqlClientFactory supabaseClientFactory)
    {
        _supabaseClientFactory = supabaseClientFactory;
    }

    public async Task<Summary?> GetSummaryByIdAsync(Guid summaryId)
    {
        var supabase = await _supabaseClientFactory.CreateClient();
        var response = await supabase.From<Summary>().Where(s => s.Id == summaryId).Get();
        return response.Models.FirstOrDefault();
    }

    public async Task<(List<Summary> Summaries, long TotalCount)> GetUserSummariesPaginatedAsync(Guid userId, int page, int pageSize)
    {
        var supabase = await _supabaseClientFactory.CreateClient();
        var countResponse = await supabase.From<Summary>().Where(s => s.UserId == userId).Count(global::Supabase.Postgrest.Constants.CountType.Exact);
        long totalCount = countResponse;
        int itemsToSkip = (page - 1) * pageSize;
        var summariesResponse = await supabase.From<Summary>()
            .Where(s => s.UserId == userId)
            .Order("generation_date", global::Supabase.Postgrest.Constants.Ordering.Descending)
            .Range(itemsToSkip, itemsToSkip + pageSize - 1)
            .Get();
        return (summariesResponse.Models, totalCount);
    }

    public async Task<Summary?> InsertSummaryAsync(Summary summary)
    {
        var supabase = await _supabaseClientFactory.CreateClient();
        var insertResponse = await supabase.From<Summary>().Insert(summary);
        return insertResponse.Models.FirstOrDefault();
    }

    public async Task<List<Note>> GetNotesForSummaryAsync(Guid userId, string period, DateTimeOffset? periodStart = null, DateTimeOffset? periodEnd = null)
    {
        var supabase = await _supabaseClientFactory.CreateClient();
        var query = supabase.From<Note>().Where(n => n.UserId == userId);
        if (periodStart.HasValue)
            query = query.Where(n => n.CreationDate >= periodStart.Value);
        if (periodEnd.HasValue)
            query = query.Where(n => n.CreationDate <= periodEnd.Value);
        var response = await query.Get();
        return response.Models;
    }
}
