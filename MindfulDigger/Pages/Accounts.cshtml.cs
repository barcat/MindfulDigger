using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MindfulDigger.Services;
using System.Security.Claims;

namespace MindfulDigger.Pages
{
    public class AccountsModel : PageModel
    {
        private readonly IAuthService _authService;

        public AccountsModel(IAuthService authService)
        {
            _authService = authService;
        }

        public string? UserEmail { get; set; }

        public void OnGet()
        {
            UserEmail = User.FindFirstValue(ClaimTypes.Email);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _authService.Logout();
            return RedirectToPage("/Login");
        }
    }
}
