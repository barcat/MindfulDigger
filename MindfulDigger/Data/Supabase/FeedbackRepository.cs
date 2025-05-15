using MindfulDigger.Models;
using MindfulDigger.Services;
using Supabase;

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
        var response = await supabase.From<Feedback>().Upsert(feedback);
        return response.Models.FirstOrDefault();
    }
}
