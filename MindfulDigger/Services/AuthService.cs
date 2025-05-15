using MindfulDigger.DTOs;
using Supabase;
using Supabase.Gotrue.Exceptions; // Corrected namespace
using Microsoft.Extensions.Options;

namespace MindfulDigger.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly SupabaseSettings _supabaseSettings;

        public AuthService(IAuthRepository authRepository, ILogger<AuthService> logger, IOptions<SupabaseSettings> supabaseOptions)
        {
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _supabaseSettings = supabaseOptions?.Value ?? throw new ArgumentNullException(nameof(supabaseOptions));
        }

        public async Task<(string? Token, string? UserId, string? RefreshToken)> LoginAsync(LoginRequestDto loginRequest)
        {
            if (loginRequest == null)
            {
                _logger.LogError("Login request is null.");
                throw new ArgumentNullException(nameof(loginRequest));
            }
            if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                _logger.LogWarning("Email or password was not provided.");
                throw new ArgumentException("Email and password are required.");
            }
            try
            {
                var result = await _authRepository.LoginAsync(loginRequest);
                if (result.Token == null || result.UserId == null)
                {
                    _logger.LogWarning("Sign in failed for user {Email}.", loginRequest.Email);
                }
                else
                {
                    _logger.LogInformation("User {Email} signed in successfully.", loginRequest.Email);
                }
                return result;
            }
            catch (GotrueException ex)
            {
                _logger.LogError(ex, "Supabase authentication error for user {Email}: {ErrorMessage}", loginRequest.Email, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login for user {Email}.", loginRequest.Email);
                throw;
            }
        }

        public async Task RegisterUser(RegistrationDto registrationDto)
        {
            if (registrationDto == null)
                throw new ArgumentNullException(nameof(registrationDto));
            if (string.IsNullOrWhiteSpace(registrationDto.Email))
                throw new ArgumentException("Adres e-mail jest wymagany.");
            if (string.IsNullOrWhiteSpace(registrationDto.Password))
                throw new ArgumentException("Hasło jest wymagane.");
            if (registrationDto.Password != registrationDto.ConfirmPassword)
                throw new ArgumentException("Hasła nie są zgodne.");
            if (!registrationDto.Email.Contains("@"))
                throw new ArgumentException("Nieprawidłowy format adresu e-mail.");
            if (registrationDto.Password.Length < 8)
                throw new ArgumentException("Hasło musi mieć co najmniej 8 znaków.");
            try
            {
                await _authRepository.RegisterUserAsync(registrationDto);
            }
            catch (GotrueException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd rejestracji użytkownika przez Supabase.");
                throw new Exception("Wystąpił błąd podczas rejestracji. Spróbuj ponownie później.");
            }
        }

        public async Task SendForgotPasswordEmail(ForgotPasswordDto forgotPasswordDto)
        {
            if (forgotPasswordDto == null)
                throw new ArgumentNullException(nameof(forgotPasswordDto));
            if (string.IsNullOrWhiteSpace(forgotPasswordDto.Email))
                throw new ArgumentException("Adres e-mail jest wymagany.");
            if (!forgotPasswordDto.Email.Contains("@"))
                throw new ArgumentException("Nieprawidłowy format adresu e-mail.");
            try
            {
                await _authRepository.SendForgotPasswordEmailAsync(forgotPasswordDto);
            }
            catch (GotrueException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd wysyłki e-maila resetującego hasło przez Supabase.");
                throw new Exception("Wystąpił błąd podczas wysyłania e-maila. Spróbuj ponownie później.");
            }
        }

        public async Task ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto == null)
                throw new ArgumentNullException(nameof(resetPasswordDto));
            if (string.IsNullOrWhiteSpace(resetPasswordDto.Email))
                throw new ArgumentException("Adres e-mail jest wymagany.");
            if (string.IsNullOrWhiteSpace(resetPasswordDto.Token))
                throw new ArgumentException("Token resetu hasła jest wymagany.");
            if (string.IsNullOrWhiteSpace(resetPasswordDto.NewPassword))
                throw new ArgumentException("Nowe hasło jest wymagane.");
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
                throw new ArgumentException("Hasła nie są zgodne.");
            if (resetPasswordDto.NewPassword.Length < 8)
                throw new ArgumentException("Hasło musi mieć co najmniej 8 znaków.");
            try
            {
                await _authRepository.ResetPasswordAsync(resetPasswordDto, _supabaseSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd resetowania hasła przez Supabase.");
                throw new Exception("Wystąpił błąd podczas resetowania hasła. Spróbuj ponownie później.");
            }
        }
    }
}
