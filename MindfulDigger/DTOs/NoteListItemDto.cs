using System;

namespace MindfulDigger.DTOs;

public class NoteListItemDto
{
    public required string Id { get; set; }
    public DateTime CreationDate { get; set; }
    public required string ContentSnippet { get; set; }
}