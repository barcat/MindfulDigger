using System;

namespace MindfulDigger.Model
{
    public class NoteDetailDto
    {
        public required string Id { get; set; }
        public required string Content { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
