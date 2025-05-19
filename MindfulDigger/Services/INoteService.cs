using MindfulDigger.Model;

namespace MindfulDigger.Services;

public interface INoteService
{
    Task<CreateNoteResponse> CreateNoteAsync(CreateNoteRequest request, Guid userId, string jwt, string refreshToken);
    // Define custom exception for note limit
    public class UserNoteLimitExceededException : Exception
    {
        public UserNoteLimitExceededException(string message) : base(message) { }
    }

    Task<PaginatedResponse<NoteListItemDto>> GetUserNotesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken,
        string jwt,
        string refreshToken
    );

    Task<Note?> GetNoteByIdAsync(Guid noteId, Guid userId, string jwt, string refreshToken);

    Task<bool> DeleteNoteAsync(Guid noteId, Guid userId, string jwt, string refreshToken);
    // Nowa metoda do implementacji w NoteService
    Task<bool> UserOwnsNoteAsync(Guid noteId, Guid userId, string jwt, string refreshToken);
}
