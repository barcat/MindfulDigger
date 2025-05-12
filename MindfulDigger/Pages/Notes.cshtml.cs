using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MindfulDigger.DTOs;
using MindfulDigger.Services;
using System.Security.Claims;

namespace MindfulDigger.Pages
{
    //[Authorize]
    public class NotesModel : PageModel
    {
        private readonly INoteService _noteService;

        public NotesModel(INoteService noteService)
        {
            _noteService = noteService;
        }

        public PaginatedResponse<NoteListItemDto> NotesResponse { get; set; } = new();
        public IEnumerable<NoteListItemDto> Notes => NotesResponse?.Items ?? Array.Empty<NoteListItemDto>();
        public int TotalCount => NotesResponse?.Pagination?.TotalCount ?? 0;
        public bool HasReachedLimit => TotalCount >= 100;

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            try
            {
                var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
                var jwt = User.FindFirstValue("AccessToken") ?? string.Empty;
                var refreshToken = User.FindFirstValue("RefreshToken") ?? string.Empty;

                if (string.IsNullOrEmpty(jwt))
                {
                    return RedirectToPage("/Login");
                }

                NotesResponse = await _noteService.GetUserNotesAsync(userId, 1, 15, cancellationToken, jwt, refreshToken);
                return Page();
            }
            catch (Exception)
            {
                // W przypadku błędu, zatrzymaj aplikację przez ponowne wyrzucenie wyjątku
                throw;
            }
        }
    }
}
