using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("summaries")]
public class Summary : BaseModel
{
    [PrimaryKey("id")]
    public string? Id { get; set; } // Made nullable

    [Column("user_id")]
    public required string UserId { get; set; } // Added required

    [Column("content")]
    public required string Content { get; set; } // Added required

    [Column("generation_date")]
    public DateTime GenerationDate { get; set; }

    [Column("period_description")]
    public required string PeriodDescription { get; set; } // Added required

    [Column("period_start")]
    public DateTime? PeriodStart { get; set; }

    [Column("period_end")]
    public DateTime? PeriodEnd { get; set; }

    [Column("is_automatic")]
    public bool IsAutomatic { get; set; }
}