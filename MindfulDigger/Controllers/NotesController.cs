using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindfulDigger.DTOs;
using MindfulDigger.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using static MindfulDigger.Services.INoteService; // For UserNoteLimitExceededException

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

    // Placeholder for potential future GET endpoint referenced by CreatedAtAction
    // [HttpGet("{id}")]
    // public async Task<IActionResult> GetNoteById(string id)
    // {
    //     // TODO: Implement retrieval logic
    //     return NotFound(); // Placeholder
    // }
}
