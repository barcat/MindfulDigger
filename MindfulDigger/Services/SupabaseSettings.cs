namespace MindfulDigger.Services
{
    public class SupabaseSettings
    {
        public required string Url { get; set; }
        public required string Key { get; set; }
        public required string JwtSecret { get; set; } // Add JwtSecret property
    }
}
