using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MindfulDigger.Data.Supabase.Model;

[Table("notes")]
public class NoteSupabaseDbModel : BaseModel
{
    public NoteSupabaseDbModel() { }

    [PrimaryKey("id")]
    public string? Id { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; } = null!;

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    // Usunięto metody mapujące - przeniesione do NoteMapper
}
