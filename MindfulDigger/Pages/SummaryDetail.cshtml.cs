using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MindfulDigger.Model;
using MindfulDigger.Services;
using System;
using System.Threading.Tasks;
using System.Security.Claims;

namespace MindfulDigger.Pages
{
    public class SummaryDetailModel : PageModel
    {
        private readonly ISummaryService _summaryService;
        public SummaryDetailsDto? Summary { get; set; }
        public string? ErrorMessage { get; set; }

        public SummaryDetailModel(ISummaryService summaryService)
        {
            _summaryService = summaryService;
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                var jwt = User.FindFirstValue("AccessToken") ?? string.Empty;
                var refreshToken = User.FindFirstValue("RefreshToken") ?? string.Empty;
                if (string.IsNullOrEmpty(jwt))
                {
                    ErrorMessage = "Brak autoryzacji. Zaloguj się ponownie.";
                    return RedirectToPage("/Login");
                }
                var summary = await _summaryService.GetSummaryByIdAsync(id, jwt, refreshToken);
                if (summary == null)
                {
                    ErrorMessage = "Podsumowanie nie zostało znalezione.";
                }
                else
                {
                    Summary = new SummaryDetailsDto
                    {
                        Id = summary.Id,
                        UserId = summary.UserId,
                        Content = summary.Content,
                        GenerationDate = summary.GenerationDate,
                        PeriodDescription = summary.PeriodDescription,
                        PeriodStart = summary.PeriodStart,
                        PeriodEnd = summary.PeriodEnd,
                        IsAutomatic = summary.IsAutomatic,
                        Status = summary.Status
                    };
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd: {ex.Message}";
            }
            return Page();
        }
    }
}
