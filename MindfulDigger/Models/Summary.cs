namespace MindfulDigger.Models;

using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("summaries")]
public class Summary : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("content")]
    public string Content { get; set; }

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

    // Parameterless constructor to satisfy the 'new()' constraint
    // and initialize required members.
    public Summary()
    {
        // Initialize required string properties to a non-null default.
        // The actual values will be populated by Supabase client during deserialization.
        Content = string.Empty; 
        PeriodDescription = string.Empty;
    }
}