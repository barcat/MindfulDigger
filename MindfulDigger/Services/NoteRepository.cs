using MindfulDigger.DTOs;
using MindfulDigger.Models;
using Supabase;
using Supabase.Postgrest.Exceptions;

namespace MindfulDigger.Services;

public interface INoteRepository
{
    Task<long> GetUserNotesCountAsync(Guid userId, string jwt, string refreshToken);
    Task<Note> InsertNoteAsync(CreateNoteRequest request, Guid userId, string jwt, string refreshToken);
    Task<Note?> GetNoteByIdAsync(Guid noteId, Guid userId, string jwt, string refreshToken);
    Task<long> GetTotalUserNotesCountAsync(Guid userId, string jwt, string refreshToken, CancellationToken cancellationToken);
    Task<List<Note>> FetchPaginatedNotesAsync(Guid userId, int offset, int pageSize, CancellationToken cancellationToken, string jwt, string refreshToken);
}

public class NoteRepository : INoteRepository
{
    private readonly ISqlClientFactory _clientFactory;
    private readonly ILogger _logger;

    public NoteRepository(ISqlClientFactory clientFactory, ILogger logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    private async Task<Client> GetClientAsync(string jwt, string refreshToken)
    {
        var client = await _clientFactory.CreateClient();

        if (!string.IsNullOrEmpty(jwt))
            await client.Auth.SetSession(jwt, refreshToken);

        return client;
    }

    public async Task<long> GetUserNotesCountAsync(Guid userId, string jwt, string refreshToken)
    {
        try
        {
            var client = await GetClientAsync(jwt, refreshToken);
            var countResponse = await client
                .From<Note>()
                .Where(n => n.UserId == userId)
                .Count(Supabase.Postgrest.Constants.CountType.Exact);

            return countResponse;
        }
        catch (PostgrestException ex)
        {
            _logger.LogError(ex, "Error checking note count for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Note> InsertNoteAsync(CreateNoteRequest request, Guid userId, string jwt, string refreshToken)
    {
        var newNote = new Note
        {
            UserId = userId,
            Content = request.Content,
            CreationDate = DateTime.Now,
        };

        try
        {
            var client = await GetClientAsync(jwt, refreshToken);
            var insertResponse = await client.From<Note>().Insert(newNote);

            if (insertResponse.Models == null || insertResponse.Models.Count == 0)
            {
                _logger.LogError("Failed to insert note for user {UserId}. Supabase response contained no models.", userId);
                throw new Exception("Failed to create note. Database did not return the created record.");
            }

            var createdNote = insertResponse.Models.First();
            if (createdNote == null || createdNote.Id == null)
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
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during note insertion for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Note?> GetNoteByIdAsync(Guid noteId, Guid userId, string jwt, string refreshToken)
    {
        try
        {
            var client = await GetClientAsync(jwt, refreshToken);
            var noteIdString = noteId.ToString();
            var notes = await client
                .From<Note>()
                .Where(n => n.Id == noteIdString)
                .Where(n => n.UserId == userId)
                .Get();

            var response = notes.Models.FirstOrDefault();

            if (response == null)
            {
                _logger.LogWarning("Note {NoteId} not found for user {UserId}", noteId, userId);
                return null;
            }
            return response;
        }
        catch (PostgrestException ex)
        {
            _logger.LogError(ex, "Database error while fetching note {NoteId} for user {UserId}", noteId, userId);
            throw;
        }
    }

    public async Task<long> GetTotalUserNotesCountAsync(Guid userId, string jwt, string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var client = await GetClientAsync(jwt, refreshToken);
            var countQuery = client
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

    public async Task<List<Note>> FetchPaginatedNotesAsync(Guid userId, int offset, int pageSize, CancellationToken cancellationToken, string jwt, string refreshToken)
    {
        try
        {
            var client = await GetClientAsync(jwt, refreshToken);
            var notesQueryBuilder = client
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
}
