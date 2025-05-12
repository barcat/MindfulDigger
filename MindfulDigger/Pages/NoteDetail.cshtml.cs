using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MindfulDigger.Services;
using System.Security.Claims;

namespace MindfulDigger.Pages
{
    public class NoteDetailModel : PageModel
    {
        private readonly INoteService _noteService;
        
        public NoteDetailModel(INoteService noteService)
        {
            _noteService = noteService;
        }

        public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("/Notes");
            }

            try
            {
                var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
                var jwt = User.FindFirstValue("AccessToken") ?? string.Empty;
                var refreshToken = User.FindFirstValue("RefreshToken") ?? string.Empty;

                if (string.IsNullOrEmpty(jwt))
                {
                    return RedirectToPage("/Login");
                }

                // Initial API call will be handled by frontend to avoid duplicate requests
                return Page();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
