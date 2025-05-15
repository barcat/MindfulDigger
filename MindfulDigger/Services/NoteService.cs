using MindfulDigger.DTOs;
using MindfulDigger.Models;
using Supabase;
using Supabase.Postgrest.Exceptions;


namespace MindfulDigger.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;
    private readonly ILogger<NoteService> _logger;
    private const int MaxNotesPerUser = 100;
    private const int SnippetLength = 120;

    public NoteService(INoteRepository noteRepository, ILogger<NoteService> logger)
    {
        _noteRepository = noteRepository;
        _logger = logger;
    }

    public async Task<CreateNoteResponse> CreateNoteAsync(CreateNoteRequest request, Guid userId, string jwt, string refreshToken)
    {
        _logger.LogInformation("Attempting to create note for user {UserId}", userId);


        await ValidateNoteLimitAsync(userId, jwt, refreshToken);
        var createdNote = await _noteRepository.InsertNoteAsync(request, userId, jwt, refreshToken);
        return MapToCreateNoteResponse(createdNote);
    }

    public async Task<PaginatedResponse<NoteListItemDto>> GetUserNotesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken,
        string jwt, 
        string refreshToken)
    {
        _logger.LogInformation("Fetching notes for user {UserId}, page {Page}, pageSize {PageSize}",
            userId, page, pageSize);

        try
        {
            var offset = (page - 1) * pageSize;

            var totalCount = await _noteRepository.GetTotalUserNotesCountAsync(userId, jwt, refreshToken, cancellationToken);
            var notes = await _noteRepository.FetchPaginatedNotesAsync(userId, offset, pageSize, cancellationToken, jwt, refreshToken);
            var noteDtos = MapNotesToListItemDtos(notes);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PaginatedResponse<NoteListItemDto>
            {
                Items = noteDtos,
                Pagination = new PaginationMetadataDto
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalCount = (int)totalCount
                }
            };
        }
        catch (Exception ex) when (ex is not PostgrestException) 
        {
            _logger.LogError(ex, "Unexpected error while fetching notes for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Note?> GetNoteByIdAsync(Guid noteId, Guid userId, string jwt, string refreshToken)
    {
        _logger.LogInformation("Fetching note {NoteId} for user {UserId}", noteId, userId);
        try
        {
            return await _noteRepository.GetNoteByIdAsync(noteId, userId, jwt, refreshToken);
        }
        catch (PostgrestException ex)
        {
            _logger.LogError(ex, "Database error while fetching note {NoteId} for user {UserId}", noteId, userId);
            throw;
        }
    }

    private async Task ValidateNoteLimitAsync(Guid userId, string jwt, string refreshToken)
    {
        try
        {
            var countResponse = await _noteRepository.GetUserNotesCountAsync(userId, jwt, refreshToken);

            if (countResponse >= MaxNotesPerUser)
            {
                _logger.LogWarning("User {UserId} has reached the note limit of {MaxNotes}", userId, MaxNotesPerUser);
                throw new INoteService.UserNoteLimitExceededException($"User has reached the maximum limit of {MaxNotesPerUser} notes.");
            }
            _logger.LogInformation("User {UserId} has {NoteCount} notes, limit is {MaxNotes}", userId, countResponse, MaxNotesPerUser);
        }
        catch (PostgrestException ex)
        {
            _logger.LogError(ex, "Error checking note count for user {UserId}", userId);
            throw;
        }
    }

    private CreateNoteResponse MapToCreateNoteResponse(Note createdNote)
    {
        var contentSnippet = createdNote.Content.Length > SnippetLength
            ? string.Concat(createdNote.Content.AsSpan(0, SnippetLength), "...")
            : createdNote.Content;

        return new CreateNoteResponse
        {
            Id = createdNote.Id!,
            UserId = createdNote.UserId,
            Content = createdNote.Content,
            CreationDate = createdNote.CreationDate,
            ContentSnippet = contentSnippet
        };
    }

    private List<NoteListItemDto> MapNotesToListItemDtos(List<Note> notes)
    {
        return notes.Select(n => new NoteListItemDto
        {
            Id = n.Id!,
            CreationDate = n.CreationDate,
            ContentSnippet = n.Content.Length <= SnippetLength
                ? n.Content
                : n.Content[..SnippetLength] + "..."
        }).ToList();
    }
}
