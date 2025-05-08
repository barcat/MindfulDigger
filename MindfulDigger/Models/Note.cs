namespace MindfulDigger.Models;

using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("notes")]
public class Note : BaseModel
{
    // Explicit parameterless constructor to satisfy the 'new()' constraint
    public Note() { }

    [PrimaryKey("id")]
    public string? Id { get; set; } // Nullable

    [Column("user_id")]
    public Guid? UserId { get; set; } = null!; // Remove 'required', initialize with null! or ensure set before use

    [Column("content")]
    public string Content { get; set; } = null!; // Remove 'required', initialize with null! or ensure set before use

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }
}