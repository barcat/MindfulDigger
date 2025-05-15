using MindfulDigger.DTOs;
using MindfulDigger.Models;
using MindfulDigger.Services;
using Supabase.Postgrest.Exceptions;

namespace MindfulDigger.Data.Supabase;

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

    private async Task<global::Supabase.Client> GetClientAsync(string jwt, string refreshToken)
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
                .Count(global::Supabase.Postgrest.Constants.CountType.Exact);
            return countResponse;
        }
        catch (PostgrestException ex)
        {
            _logger.LogError(ex, "Error getting user notes count");
            throw;
        }
    }

    public async Task<Note> InsertNoteAsync(CreateNoteRequest request, Guid userId, string jwt, string refreshToken)
    {
        var client = await GetClientAsync(jwt, refreshToken);
        var note = new Note
        {
            // Map fields from request to Note
            // ...
        };
        var response = await client.From<Note>().Insert(note);
        return response.Models.FirstOrDefault()!;
    }

    public async Task<Note?> GetNoteByIdAsync(Guid noteId, Guid userId, string jwt, string refreshToken)
    {
        var client = await GetClientAsync(jwt, refreshToken);
        var response = await client.From<Note>().Where(n => n.Id == noteId.ToString() && n.UserId == userId).Get();
        return response.Models.FirstOrDefault();
    }

    public async Task<long> GetTotalUserNotesCountAsync(Guid userId, string jwt, string refreshToken, CancellationToken cancellationToken)
    {
        var client = await GetClientAsync(jwt, refreshToken);
        var countResponse = await client.From<Note>().Where(n => n.UserId == userId).Count(global::Supabase.Postgrest.Constants.CountType.Exact);
        return countResponse;
    }

    public async Task<List<Note>> FetchPaginatedNotesAsync(Guid userId, int offset, int pageSize, CancellationToken cancellationToken, string jwt, string refreshToken)
    {
        var client = await GetClientAsync(jwt, refreshToken);
        var response = await client.From<Note>()
            .Where(n => n.UserId == userId)
            .Order("created_at", global::Supabase.Postgrest.Constants.Ordering.Descending)
            .Range(offset, offset + pageSize - 1)
            .Get();
        return response.Models;
    }
}
