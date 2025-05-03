using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration; // Dodaj using dla IConfiguration
using Microsoft.Extensions.Logging; // Zachowaj istniejący using dla ILogger

namespace MindfulDigger.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration; // Dodaj pole dla IConfiguration

        // Dodaj właściwości do przechowywania wartości Supabase
        public string? SupabaseUrl { get; private set; }
        public bool SupabaseKeyExists { get; private set; }

        // Zmodyfikuj konstruktor, aby przyjmował IConfiguration
        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration; // Przypisz wstrzykniętą konfigurację
        }

        public void OnGet()
        {
            // Odczytaj wartości z konfiguracji
            SupabaseUrl = _configuration["Supabase:Url"];
            string? supabaseKey = _configuration["Supabase:Key"];
            SupabaseKeyExists = !string.IsNullOrEmpty(supabaseKey); // Sprawdź, czy klucz istnieje

            // Opcjonalnie: Logowanie wartości (uważaj na klucz w logach produkcyjnych)
            _logger.LogInformation("Supabase URL from config: {SupabaseUrl}", SupabaseUrl);
            _logger.LogInformation("Supabase Key exists in config: {SupabaseKeyExists}", SupabaseKeyExists);

            // Opcjonalnie: Spróbuj zainicjalizować klienta Supabase (wymaga dodania pakietu supabase-csharp i konfiguracji w Program.cs)
            // try
            // {
            //     var url = SupabaseUrl;
            //     var key = _configuration["Supabase:Key"]; // Odczytaj ponownie lub użyj zmiennej supabaseKey
            //     if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(key))
            //     {
            //         // To jest tylko przykład, rzeczywista inicjalizacja może być w Program.cs
            //         // await Supabase.Client.InitializeAsync(url, key);
            //         // ViewBag.SupabaseConnectionStatus = "Successfully initialized (simulated)";
            //     }
            //     else {
            //         // ViewBag.SupabaseConnectionStatus = "URL or Key missing in config.";
            //     }
            // }
            // catch (Exception ex)
            // {
            //     _logger.LogError(ex, "Failed to initialize Supabase client.");
            //     // ViewBag.SupabaseConnectionStatus = $"Error: {ex.Message}";
            // }
        }
    }
}
