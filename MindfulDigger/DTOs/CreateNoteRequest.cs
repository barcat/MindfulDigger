using System.ComponentModel.DataAnnotations;

namespace MindfulDigger.DTOs;

public class CreateNoteRequest
{
    [Required]
    [MaxLength(1000)]
    public required string Content { get; set; }
}
