using Microsoft.AspNetCore.Mvc;
using MindfulDigger.Model;
using MindfulDigger.Services;
using System.Security.Claims;
using static MindfulDigger.Services.INoteService;

namespace MindfulDigger.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // Require authentication for all actions in this controller
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;
    private readonly ILogger<NotesController> _logger;
    private const int DefaultPageSize = 15;
    private const int MaxPageSize = 100;

    public NotesController(INoteService noteService, ILogger<NotesController> logger)
    {
        _noteService = noteService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<NoteListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<NoteListItemDto>>> GetNotes(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        // Validate and normalize pagination parameters
        var normalizedPage = page ?? 1;
        var normalizedPageSize = pageSize ?? DefaultPageSize;

        if (normalizedPage <= 0)
        {
            return BadRequest(new { message = "Page must be greater than 0" });
        }

        if (normalizedPageSize <= 0)
        {
            return BadRequest(new { message = "PageSize must be greater than 0" });
        }

        if (normalizedPageSize > MaxPageSize)
        {
            return BadRequest(new { message = $"PageSize cannot exceed {MaxPageSize}" });
        }

        var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
        var jwt = User.FindFirstValue("AccessToken") ?? string.Empty;
        var refreshToken = User.FindFirstValue("RefreshToken") ?? string.Empty;

        try
        {
            var result = await _noteService.GetUserNotesAsync(
                userId,
                normalizedPage,
                normalizedPageSize,
                cancellationToken,
                jwt,
                refreshToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notes for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving notes" });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateNoteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Handled by [Authorize]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateNote([FromBody] CreateNoteRequest request)
    {
        var userIdstrign = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var userId = new Guid(userIdstrign);
        var jwt = User.FindFirstValue("AccessToken") ?? string.Empty;
        var refreshToken = User.FindFirstValue("RefreshToken") ?? string.Empty;

        try
        {
            _logger.LogInformation("User {UserId} attempting to create a note.", userId);
            var createdNoteDto = await _noteService.CreateNoteAsync(request, userId, jwt, refreshToken);

            // Return 201 Created with the created object
            // Using StatusCode directly as GetNoteById is not implemented yet
            return StatusCode(StatusCodes.Status201Created, createdNoteDto);
        }
        catch (UserNoteLimitExceededException ex)
        {
            _logger.LogWarning(ex, "User {UserId} exceeded note limit.", userId);
            // Return 400 Bad Request with the specific error message from the exception
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating note for user {UserId}.", userId);
            // Return 500 Internal Server Error for any other exceptions
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while creating the note." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNoteById(string id)
    {
        _logger.LogInformation("Attempting to retrieve note {NoteId}", id);

        if (!Guid.TryParse(id, out var noteId))
        {
            _logger.LogWarning("Invalid note ID format received: {InvalidId}", id);
            return BadRequest(new { message = "Invalid note id format." });
        }

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in claims");
            return Unauthorized();
        }

        var jwt = User.FindFirstValue("AccessToken") ?? string.Empty;
        var refreshToken = User.FindFirstValue("RefreshToken") ?? string.Empty;

        if (string.IsNullOrEmpty(jwt) || string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Missing authentication tokens for user {UserId}", userId);
            return Unauthorized();
        }

        try
        {
            _logger.LogDebug("Fetching note {NoteId} for user {UserId}", noteId, userId);
            var note = await _noteService.GetNoteByIdAsync(noteId, userId, jwt, refreshToken);
            
            if (note == null)
            {
                _logger.LogInformation("Note {NoteId} not found for user {UserId}", noteId, userId);
                return NotFound();
            }
            
            // Map to DTO for consistent JSON naming
            var noteDto = new NoteDetailDto
            {
                Id = note.Id!,
                Content = note.Content,
                CreationDate = note.CreationDate
            };
            
            return Ok(noteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving note {NoteId} for user {UserId}. Error details: {ErrorMessage}", 
                noteId, userId, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving the note." });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> supabemetoda(string id)
    {
        _logger.LogInformation("Attempting to delete note {NoteId}", id);

        if (!Guid.TryParse(id, out var noteId))
        {
            _logger.LogWarning("Invalid note ID format received: {InvalidId}", id);
            return BadRequest(new { message = "Invalid note id format." });
        }

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in claims");
            return Unauthorized();
        }

        var jwt = User.FindFirstValue("AccessToken") ?? string.Empty;
        var refreshToken = User.FindFirstValue("RefreshToken") ?? string.Empty;

        if (string.IsNullOrEmpty(jwt) || string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Missing authentication tokens for user {UserId}", userId);
            return Unauthorized();
        }

        try
        {
            var deleted = await _noteService.DeleteNoteAsync(noteId, userId, jwt, refreshToken);
            if (!deleted)
            {
                _logger.LogWarning("Note {NoteId} not found or could not be deleted for user {UserId}", noteId, userId);
                return NotFound();
            }

            _logger.LogInformation("Note {NoteId} deleted for user {UserId}", noteId, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting note {NoteId} for user {UserId}. Error details: {ErrorMessage}", noteId, userId, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the note." });
        }
    }
}
