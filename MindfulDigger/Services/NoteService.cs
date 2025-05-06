using MindfulDigger.DTOs;
using MindfulDigger.Models;
using Supabase;
using Microsoft.Extensions.Logging;
using Supabase.Postgrest.Exceptions;

namespace MindfulDigger.Services;

public class NoteService : INoteService
{
    private readonly Client _supabaseClient;
    private readonly ILogger<NoteService> _logger;
    private const int MaxNotesPerUser = 100; // Define the note limit
    private const int SnippetLength = 50; // Define snippet length

    public NoteService(Client supabaseClient, ILogger<NoteService> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    public async Task<CreateNoteResponse> CreateNoteAsync(CreateNoteRequest request, string userId)
    {
        _logger.LogInformation("Attempting to create note for user {UserId}", userId);

        // Step 6: Validate Note Limit
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
            // Re-throw or handle as appropriate for your error strategy
            // For now, re-throwing to be caught by the controller
            throw;
        }


        // Step 7 & 8: Create and Insert Note
        var newNote = new Note
        {
            UserId = userId,
            Content = request.Content
            // Id and CreationDate will be set by Supabase/DB
        };

        Note? createdNote = null; // Initialize as nullable
        try
        {
            var insertResponse = await _supabaseClient.From<Note>().Insert(newNote);

            if (insertResponse.Models == null || insertResponse.Models.Count == 0)
            {
                _logger.LogError("Failed to insert note for user {UserId}. Supabase response contained no models.", userId);
                throw new Exception("Failed to create note. Database did not return the created record.");
            }

            createdNote = insertResponse.Models.First();
            _logger.LogInformation("Successfully created note with ID {NoteId} for user {UserId}", createdNote.Id, userId);
        }
        catch (PostgrestException ex)
        {
            _logger.LogError(ex, "Error inserting note for user {UserId}", userId);
            // Re-throw to be caught by the controller
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during note insertion for user {UserId}", userId);
            throw; // Re-throw other exceptions
        }

        // Step 10: Map to DTO
        if (createdNote == null || createdNote.Id == null)
        {
            // This should ideally not happen if insertion succeeded and returned a model
            _logger.LogError("Created note or its ID is null after successful insertion for user {UserId}", userId);
            throw new Exception("Failed to retrieve complete note data after creation.");
        }

        var contentSnippet = createdNote.Content.Length > SnippetLength
            ? createdNote.Content.Substring(0, SnippetLength) + "..."
            : createdNote.Content;

        return new CreateNoteResponse
        {
            Id = createdNote.Id,
            UserId = createdNote.UserId,
            Content = createdNote.Content,
            CreationDate = createdNote.CreationDate,
            ContentSnippet = contentSnippet
        };
    }
}
