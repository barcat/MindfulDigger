using MindfulDigger.Data.Supabase.Model;
using System.Collections.Generic;
using MindfulDigger.Model;

namespace MindfulDigger.Data;

public static class SummaryMapper
{
    public static SummarySupabaseDbModel ToSupabaseDbModel(Summary model)
    {
        return new SummarySupabaseDbModel
        {
            Id = model.Id,
            UserId = model.UserId,
            Content = model.Content,
            GenerationDate = model.GenerationDate,
            PeriodDescription = model.PeriodDescription,
            PeriodStart = model.PeriodStart,
            PeriodEnd = model.PeriodEnd,
            IsAutomatic = model.IsAutomatic,
            //Status = model.Status,
            //CreatedAt = model.CreatedAt
        };
    }

    public static Summary ToModel(SummarySupabaseDbModel dbModel)
    {
        return new Summary
        {
            Id = dbModel.Id,
            UserId = dbModel.UserId,
            Content = dbModel.Content,
            GenerationDate = dbModel.GenerationDate,
            PeriodDescription = dbModel.PeriodDescription,
            PeriodStart = dbModel.PeriodStart,
            PeriodEnd = dbModel.PeriodEnd,
            IsAutomatic = dbModel.IsAutomatic,
            //Status = dbModel.Status,
            //CreatedAt = dbModel.CreatedAt
        };
    }

    public static List<Summary> ToModelList(IEnumerable<SummarySupabaseDbModel> dbModels)
    {
        var list = new List<Summary>();
        foreach (var dbModel in dbModels)
            list.Add(ToModel(dbModel));
        return list;
    }

    public static List<SummarySupabaseDbModel> ToSupabaseDbModelList(IEnumerable<Summary> models)
    {
        var list = new List<SummarySupabaseDbModel>();
        foreach (var model in models)
            list.Add(ToSupabaseDbModel(model));
        return list;
    }
}
