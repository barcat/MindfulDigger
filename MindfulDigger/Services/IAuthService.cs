using MindfulDigger.Model;

namespace MindfulDigger.Services
{
    public interface IAuthService
    {
        Task<(string? Token, string? UserId, string? RefreshToken)> LoginAsync(LoginRequestDto loginRequest);
        Task RegisterUser(RegistrationDto registrationDto);
        Task SendForgotPasswordEmail(ForgotPasswordDto forgotPasswordDto);
        Task ResetPassword(ResetPasswordDto resetPasswordDto);
        Task Logout();
    }
}
