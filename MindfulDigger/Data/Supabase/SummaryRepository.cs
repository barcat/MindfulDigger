using MindfulDigger.Data.Supabase.Model;
using MindfulDigger.Data;
using Supabase;
using MindfulDigger.Services;
using MindfulDigger.Model;

namespace MindfulDigger.Data.Supabase;

public interface ISummaryRepository
{
    Task<Summary?> GetSummaryByIdAsync(Guid summaryId, string jwt, string refreshToken);
    Task<(List<Summary> Summaries, long TotalCount)> GetUserSummariesPaginatedAsync(Guid userId, int page, int pageSize, string jwt, string refreshToken);
    Task<Summary?> InsertSummaryAsync(Summary summary, string jwt, string refreshToken);
    Task<List<Note>> GetNotesForSummaryAsync(Guid userId, string period, DateTimeOffset? periodStart, DateTimeOffset? periodEnd, string jwt, string refreshToken);
}

public class SummaryRepository : ISummaryRepository
{
    private readonly ISqlClientFactory _supabaseClientFactory;

    public SummaryRepository(ISqlClientFactory supabaseClientFactory)
    {
        _supabaseClientFactory = supabaseClientFactory;
    }

    private async Task<global::Supabase.Client> GetClientAsync(string jwt, string refreshToken)
    {
        var client = await _supabaseClientFactory.CreateClient();
        await client.Auth.SetSession(jwt, refreshToken);
        return client;
    }

    public async Task<Summary?> GetSummaryByIdAsync(Guid summaryId, string jwt, string refreshToken)
    {
        var supabase = await GetClientAsync(jwt, refreshToken);
        var response = await supabase.From<SummarySupabaseDbModel>().Where(s => s.Id == summaryId).Get();
        var dbModel = response.Models.FirstOrDefault();
        return dbModel != null ? SummaryMapper.ToModel(dbModel) : null;
    }

    public async Task<(List<Summary> Summaries, long TotalCount)> GetUserSummariesPaginatedAsync(Guid userId, int page, int pageSize, string jwt, string refreshToken)
    {
        var supabase = await GetClientAsync(jwt, refreshToken);
        var countResponse = await supabase.From<SummarySupabaseDbModel>().Where(s => s.UserId == userId).Count(global::Supabase.Postgrest.Constants.CountType.Exact);
        long totalCount = countResponse;
        int itemsToSkip = (page - 1) * pageSize;
        var summariesResponse = await supabase.From<SummarySupabaseDbModel>()
            .Where(s => s.UserId == userId)
            .Order("generation_date", global::Supabase.Postgrest.Constants.Ordering.Descending)
            .Range(itemsToSkip, itemsToSkip + pageSize - 1)
            .Get();
        var summaries = SummaryMapper.ToModelList(summariesResponse.Models);
        return (summaries, totalCount);
    }

    public async Task<Summary?> InsertSummaryAsync(Summary summary, string jwt, string refreshToken)
    {
        var supabase = await GetClientAsync(jwt, refreshToken);
        var dbModel = SummaryMapper.ToSupabaseDbModel(summary);
        var insertResponse = await supabase.From<SummarySupabaseDbModel>().Insert(dbModel);
        var inserted = insertResponse.Models.FirstOrDefault();
        return inserted != null ? SummaryMapper.ToModel(inserted) : null;
    }

    public async Task<List<Note>> GetNotesForSummaryAsync(Guid userId, string period, DateTimeOffset? periodStart, DateTimeOffset? periodEnd, string jwt, string refreshToken)
    {
        var supabase = await GetClientAsync(jwt, refreshToken);
        var query = supabase.From<NoteSupabaseDbModel>()
                            .Where(n => n.UserId == userId);
        
        if (periodStart.HasValue)
            query = query.Where(n => n.CreationDate >= periodStart.Value);
        
        if (periodEnd.HasValue)
            query = query.Where(n => n.CreationDate <= periodEnd.Value);
        
        var response = await query.Get();
        
        return NoteMapper.ToModelList(response.Models);
    }
}
