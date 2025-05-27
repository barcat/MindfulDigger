using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MindfulDigger.Data.Supabase.Model;

public enum FeedbackRatingSupabaseDbModel
{
    Positive,
    Negative
}

[Table("feedback")]
public class FeedbackSupabaseDbModel : BaseModel
{
    [PrimaryKey("summary_id, user_id", true)]
    public string? SummaryId { get; set; }

    [Column("user_id")]
    public string? UserId { get; set; }

    [Column("rating")]
    [JsonConverter(typeof(StringEnumConverter))]
    public FeedbackRatingSupabaseDbModel Rating { get; set; }

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    // Usunięto metody mapujące - przeniesione do FeedbackMapper
}
