using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MindfulDigger.Data.Supabase.Model;

[Table("summaries")]
public class SummarySupabaseDbModel : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("content")]
    public string? Content { get; set; }

    [Column("generation_date")]
    public DateTimeOffset GenerationDate { get; set; }

    [Column("period_description")]
    public string PeriodDescription { get; set; }

    [Column("period_start")]
    public DateTimeOffset? PeriodStart { get; set; }

    [Column("period_end")]
    public DateTimeOffset? PeriodEnd { get; set; }

    [Column("is_automatic")]
    public bool IsAutomatic { get; set; }

    //[Column("status")]
    //public string? Status { get; set; }

    //[Column("created_at")]
    //public DateTime CreatedAt { get; set; }

    public SummarySupabaseDbModel()
    {
        PeriodDescription = string.Empty;
    }

    // Usunięto metody mapujące - przeniesione do SummaryMapper
}
