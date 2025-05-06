using MindfulDigger.DTOs;

namespace MindfulDigger.Services;

public interface INoteService
{
    Task<CreateNoteResponse> CreateNoteAsync(CreateNoteRequest request, string userId);
    // Define custom exception for note limit
    public class UserNoteLimitExceededException : Exception
    {
        public UserNoteLimitExceededException(string message) : base(message) { }
    }

}
