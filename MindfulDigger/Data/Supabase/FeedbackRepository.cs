using MindfulDigger.Data.Supabase.Model;
using MindfulDigger.Data;
using MindfulDigger.Services;
using Supabase;
using MindfulDigger.Model;

namespace MindfulDigger.Data.Supabase;

public interface IFeedbackRepository
{
    Task<Feedback?> UpsertFeedbackAsync(Feedback feedback);
}

public class FeedbackRepository : IFeedbackRepository
{
    private readonly ISqlClientFactory _supabaseClientFactory;

    public FeedbackRepository(ISqlClientFactory supabaseClientFactory)
    {
        _supabaseClientFactory = supabaseClientFactory;
    }

    public async Task<Feedback?> UpsertFeedbackAsync(Feedback feedback)
    {
        var supabase = await _supabaseClientFactory.CreateClient();
        var dbModel = FeedbackMapper.ToSupabaseDbModel(feedback);
        var response = await supabase.From<FeedbackSupabaseDbModel>().Upsert(dbModel);
        var inserted = response.Models.FirstOrDefault();
        return inserted != null ? FeedbackMapper.ToModel(inserted) : null;
    }
}
