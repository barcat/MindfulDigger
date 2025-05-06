using System;

namespace MindfulDigger.DTOs;

public class NoteListItemDto
{
    public string Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string ContentSnippet { get; set; }
}