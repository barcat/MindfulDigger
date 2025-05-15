using MindfulDigger.Models;
using MindfulDigger.Data.Supabase.Model;
using System.Collections.Generic;

namespace MindfulDigger.Data;

public static class NoteMapper
{
    public static NoteSupabaseDbModel ToSupabaseDbModel(Note model)
    {
        return new NoteSupabaseDbModel
        {
            Id = model.Id,
            UserId = model.UserId,
            Content = model.Content,
            CreationDate = model.CreationDate
        };
    }

    public static Note ToModel(NoteSupabaseDbModel dbModel)
    {
        return new Note
        {
            Id = dbModel.Id,
            UserId = dbModel.UserId,
            Content = dbModel.Content,
            CreationDate = dbModel.CreationDate
        };
    }

    public static List<Note> ToModelList(IEnumerable<NoteSupabaseDbModel> dbModels)
    {
        var list = new List<Note>();
        foreach (var dbModel in dbModels)
            list.Add(ToModel(dbModel));
        return list;
    }

    public static List<NoteSupabaseDbModel> ToSupabaseDbModelList(IEnumerable<Note> models)
    {
        var list = new List<NoteSupabaseDbModel>();
        foreach (var model in models)
            list.Add(ToSupabaseDbModel(model));
        return list;
    }
}
