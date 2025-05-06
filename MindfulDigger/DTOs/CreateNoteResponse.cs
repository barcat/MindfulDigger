namespace MindfulDigger.DTOs;

public class CreateNoteResponse
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string Content { get; set; }
    public DateTime CreationDate { get; set; }
    public required string ContentSnippet { get; set; }
}
