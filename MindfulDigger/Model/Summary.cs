namespace MindfulDigger.Model;

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
    public string? Content { get; set; } // Remains nullable as per plan

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

    [Column("status")]
    public string? Status { get; set; } 

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } 

    // Parameterless constructor
    public Summary()
    {
        // Content is nullable, so no default initialization here.
        PeriodDescription = string.Empty;
        // Initialize CreatedAt to a sensible default, though it will likely be set by the DB or service.
        CreatedAt = DateTime.UtcNow;
        // Status is nullable, Content is nullable.
    }
}