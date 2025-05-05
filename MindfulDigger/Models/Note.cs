using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("notes")]
public class Note : BaseModel
{
    [PrimaryKey("id")]
    public string? Id { get; set; } // Made nullable

    [Column("user_id")]
    public required string UserId { get; set; } // Added required

    [Column("content")]
    public required string Content { get; set; } // Added required

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }
}