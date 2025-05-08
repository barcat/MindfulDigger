using MindfulDigger.DTOs;
using MindfulDigger.Models;
using Supabase;
using Supabase.Postgrest.Exceptions;

namespace MindfulDigger.Services;

public class NoteService : INoteService
{
    private readonly Client _supabaseClient;
    private readonly ILogger<NoteService> _logger;
    private const int MaxNotesPerUser = 100; // Define the note limit
    private const int SnippetLength = 120; // Changed to match the implementation plan

    public NoteService(Client supabaseClient, ILogger<NoteService> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    public async Task<CreateNoteResponse> CreateNoteAsync(CreateNoteRequest request, Guid userId)
    {
        _logger.LogInformation("Attempting to create note for user {UserId}", userId);

        await ValidateNoteLimitAsync(userId);
        var createdNote = await InsertNoteInternalAsync(request, userId);
        return MapToCreateNoteResponse(createdNote);
    }

    public async Task<PaginatedResponse<NoteListItemDto>> GetUserNotesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching notes for user {UserId}, page {Page}, pageSize {PageSize}",
            userId, page, pageSize);

        try
        {
            var offset = (page - 1) * pageSize;

            var totalCount = await GetTotalUserNotesCountAsync(userId, cancellationToken);
            var notes = await FetchPaginatedNotesAsync(userId, offset, pageSize, cancellationToken);
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

    private async Task ValidateNoteLimitAsync(Guid userId)
    {
        try
        {
            var countResponse = await _supabaseClient
                .From<Note>()
                .Where(n => n.UserId == userId)
                .Count(Supabase.Postgrest.Constants.CountType.Exact);

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
            throw; // Re-throw to be caught by the calling method
        }
    }

    private async Task<Note> InsertNoteInternalAsync(CreateNoteRequest request, Guid userId)
    {
        var newNote = new Note
        {
            UserId = userId,
            Content = request.Content
            // Id and CreationDate will be set by Supabase/DB
        };

        try
        {
            var insertResponse = await _supabaseClient.From<Note>().Insert(newNote);

            if (insertResponse.Models == null || insertResponse.Models.Count == 0)
            {
                _logger.LogError("Failed to insert note for user {UserId}. Supabase response contained no models.", userId);
                throw new Exception("Failed to create note. Database did not return the created record.");
            }

            var createdNote = insertResponse.Models.First();
            if (createdNote == null || createdNote.Id == null) // Ensure createdNote and its Id are not null
            {
                _logger.LogError("Created note or its ID is null after successful insertion for user {UserId}", userId);
                throw new Exception("Failed to retrieve complete note data after creation.");
            }
            _logger.LogInformation("Successfully created note with ID {NoteId} for user {UserId}", createdNote.Id, userId);
            return createdNote;
        }
        catch (PostgrestException ex)
        {
            _logger.LogError(ex, "Error inserting note for user {UserId}", userId);
            throw; // Re-throw to be caught by the calling method
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during note insertion for user {UserId}", userId);
            throw; // Re-throw other exceptions
        }
    }

    private CreateNoteResponse MapToCreateNoteResponse(Note createdNote)
    {
        var contentSnippet = createdNote.Content.Length > SnippetLength
            ? createdNote.Content.Substring(0, SnippetLength) + "..."
            : createdNote.Content;

        return new CreateNoteResponse
        {
            Id = createdNote.Id!, // Null check for Id is done in InsertNoteInternalAsync
            UserId = createdNote.UserId,
            Content = createdNote.Content,
            CreationDate = createdNote.CreationDate,
            ContentSnippet = contentSnippet
        };
    }

    private async Task<long> GetTotalUserNotesCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var countQuery = _supabaseClient
                .From<Note>()
                .Where(n => n.UserId == userId);
            return await countQuery.Count(Supabase.Postgrest.Constants.CountType.Exact, cancellationToken);
        }
        catch (PostgrestException ex)
        {
            _logger.LogError(ex, "Database error while fetching total note count for user {UserId}", userId);
            throw;
        }
    }

    private async Task<List<Note>> FetchPaginatedNotesAsync(Guid userId, int offset, int pageSize, CancellationToken cancellationToken)
    {
        try
        {
            var notesQueryBuilder = _supabaseClient
                .From<Note>()
                .Where(n => n.UserId == userId)
                .Order(n => n.CreationDate, Supabase.Postgrest.Constants.Ordering.Descending)
                .Range(offset, offset + pageSize - 1)
                .Select("*");

            var notesResponse = await notesQueryBuilder.Get(cancellationToken);
            return notesResponse.Models;
        }
        catch (PostgrestException ex)
        {
            _logger.LogError(ex, "Database error while fetching paginated notes for user {UserId}", userId);
            throw;
        }
    }

    private List<NoteListItemDto> MapNotesToListItemDtos(List<Note> notes)
    {
        return notes.Select(n => new NoteListItemDto
        {
            Id = n.Id!, // Assuming Id is never null for existing notes
            CreationDate = n.CreationDate,
            ContentSnippet = n.Content.Length <= SnippetLength
                ? n.Content
                : n.Content[..SnippetLength] + "..."
        }).ToList();
    }
}
