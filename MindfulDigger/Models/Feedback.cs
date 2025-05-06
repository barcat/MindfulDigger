namespace MindfulDigger.Models;

using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public enum FeedbackRating
{
    Positive,
    Negative
}

[Table("feedback")]
public class Feedback : BaseModel
{
    [PrimaryKey("summary_id, user_id", true)] // Composite key
    public required string SummaryId { get; set; } // Added required

    // UserId is part of the composite key, handled by PrimaryKey
    [Column("user_id")]
    public required string UserId { get; set; } // Added required

    [Column("rating")]
    [JsonConverter(typeof(StringEnumConverter))] // Ensure enum is serialized as string
    public FeedbackRating Rating { get; set; }

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }
}