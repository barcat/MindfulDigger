namespace MindfulDigger.Model;

public class CreateNoteResponse
{
    public required string Id { get; set; }
    public required Guid? UserId { get; set; }
    public required string Content { get; set; }
    public DateTime CreationDate { get; set; }
    public required string ContentSnippet { get; set; }
}
