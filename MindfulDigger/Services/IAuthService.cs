using MindfulDigger.DTOs;

namespace MindfulDigger.Services
{
    public interface IAuthService
    {
        Task<(string? Token, string? UserId, string? RefreshToken)> LoginAsync(LoginRequestDto loginRequest);
    }
}
