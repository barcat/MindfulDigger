namespace MindfulDigger.DTOs
{
    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public string? RefreshToken { get; set; }
    }
}
