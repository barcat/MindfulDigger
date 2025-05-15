using MindfulDigger.Data.Supabase.Model;
using System.Collections.Generic;
using MindfulDigger.Model;

namespace MindfulDigger.Data;

public static class FeedbackMapper
{
    public static FeedbackSupabaseDbModel ToSupabaseDbModel(Feedback model)
    {
        return new FeedbackSupabaseDbModel
        {
            SummaryId = model.SummaryId,
            UserId = model.UserId,
            Rating = (FeedbackRatingSupabaseDbModel)model.Rating,
            CreationDate = model.CreationDate
        };
    }

    public static Feedback ToModel(FeedbackSupabaseDbModel dbModel)
    {
        return new Feedback
        {
            SummaryId = dbModel.SummaryId,
            UserId = dbModel.UserId,
            Rating = (FeedbackRating)dbModel.Rating,
            CreationDate = dbModel.CreationDate
        };
    }

    public static List<Feedback> ToModelList(IEnumerable<FeedbackSupabaseDbModel> dbModels)
    {
        var list = new List<Feedback>();
        foreach (var dbModel in dbModels)
            list.Add(ToModel(dbModel));
        return list;
    }

    public static List<FeedbackSupabaseDbModel> ToSupabaseDbModelList(IEnumerable<Feedback> models)
    {
        var list = new List<FeedbackSupabaseDbModel>();
        foreach (var model in models)
            list.Add(ToSupabaseDbModel(model));
        return list;
    }
}
