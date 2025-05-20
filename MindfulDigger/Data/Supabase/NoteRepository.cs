using MindfulDigger.Data.Supabase.Model;
using MindfulDigger.Model;
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
    Task<bool> DeleteNoteAsync(Guid noteId, Guid userId, string jwt, string refreshToken);
}

public class NoteRepository : INoteRepository
{
    private readonly ISqlClientFactory _clientFactory;
    private readonly ILogger<NoteRepository> _logger;

    public NoteRepository(ISqlClientFactory clientFactory, ILogger<NoteRepository> logger)
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
                .From<NoteSupabaseDbModel>()
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
        var noteModel = new Note
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Content = request.Content,
            CreationDate = DateTime.UtcNow
        };
        var dbModel = NoteMapper.ToSupabaseDbModel(noteModel);
        var response = await client.From<NoteSupabaseDbModel>().Insert(dbModel);
        var inserted = response.Models.FirstOrDefault();
        return inserted != null ? NoteMapper.ToModel(inserted) : null!;
    }    
    
    public async Task<Note?> GetNoteByIdAsync(Guid noteId, Guid userId, string jwt, string refreshToken)
    {
        var client = await GetClientAsync(jwt, refreshToken);
        var response = await client.From<NoteSupabaseDbModel>()
            .Filter("id", global::Supabase.Postgrest.Constants.Operator.Equals, noteId.ToString())
            .Filter("user_id", global::Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Get();
            
        var model = response.Models.FirstOrDefault();
        return model != null ? NoteMapper.ToModel(model) : null;
    }

    public async Task<long> GetTotalUserNotesCountAsync(Guid userId, string jwt, string refreshToken, CancellationToken cancellationToken)
    {
        var client = await GetClientAsync(jwt, refreshToken);
        var countResponse = await client.From<NoteSupabaseDbModel>().Where(n => n.UserId == userId).Count(global::Supabase.Postgrest.Constants.CountType.Exact);
        return countResponse;
    }

    public async Task<List<Note>> FetchPaginatedNotesAsync(Guid userId, int offset, int pageSize, CancellationToken cancellationToken, string jwt, string refreshToken)
    {
        var client = await GetClientAsync(jwt, refreshToken);
        var response = await client.From<NoteSupabaseDbModel>()
            .Where(n => n.UserId == userId)
            .Order("creation_date", global::Supabase.Postgrest.Constants.Ordering.Descending)
            .Range(offset, offset + pageSize - 1)
            .Get();

        return NoteMapper.ToModelList(response.Models);
    }

    public async Task<bool> DeleteNoteAsync(Guid noteId, Guid userId, string jwt, string refreshToken)
    {
        var noteIdStr = noteId.ToString();

        try
        {
            var client = await GetClientAsync(jwt, refreshToken);
            
            await client.From<NoteSupabaseDbModel>()
                .Where(n => n.Id == noteIdStr)
                .Delete();

            return true;
        }
        catch (PostgrestException ex)
        {
            _logger.LogError(ex, "Error deleting note {NoteId} for user {UserId}", noteId, userId);
            throw;
        }
    }
}
