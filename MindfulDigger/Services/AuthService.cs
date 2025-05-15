using MindfulDigger.DTOs;
using Supabase;
using Supabase.Gotrue.Exceptions; // Corrected namespace
using Microsoft.Extensions.Options;

namespace MindfulDigger.Services
{
    public class AuthService : IAuthService
    {
        private readonly ISqlClientFactory _clientFactory;
        private readonly ILogger<AuthService> _logger;
        private readonly SupabaseSettings _supabaseSettings;

        public AuthService(ISqlClientFactory clientFactory, ILogger<AuthService> logger, IOptions<SupabaseSettings> supabaseOptions)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _supabaseSettings = supabaseOptions?.Value ?? throw new ArgumentNullException(nameof(supabaseOptions));
        }

        public async Task<(string? Token, string? UserId, string? RefreshToken)> LoginAsync(LoginRequestDto loginRequest) // Ensure nullable return type
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

            Client? supabase = null; // Ensure nullable type for supabase client variable
            try
            {
                supabase = await _clientFactory.CreateClient();
                _logger.LogInformation("Attempting to sign in user {Email}", loginRequest.Email);

                var session = await supabase.Auth.SignIn(loginRequest.Email, loginRequest.Password);

                if (session == null || string.IsNullOrEmpty(session.AccessToken) || session.User == null || string.IsNullOrEmpty(session.User.Id))
                {
                    _logger.LogWarning("Sign in failed for user {Email}. Session, AccessToken, or UserId is null/empty.", loginRequest.Email);
                    return (null, null, null);
                }

                _logger.LogInformation("User {Email} signed in successfully.", loginRequest.Email);
                return (session.AccessToken, session.User.Id, session.RefreshToken); // Return the refresh token as well
            }
            catch (GotrueException ex) // Now correctly refers to the imported namespace
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
            // Możesz dodać więcej reguł walidacji (cyfra, znak specjalny itd.)

            try
            {
                var supabase = await _clientFactory.CreateClient();
                var response = await supabase.Auth.SignUp(registrationDto.Email, registrationDto.Password);
                if (response == null || response.User == null)
                    throw new GotrueException("Nie udało się utworzyć konta użytkownika.");
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
                var supabase = await _clientFactory.CreateClient();
                await supabase.Auth.ResetPasswordForEmail(forgotPasswordDto.Email);
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
            // Możesz dodać więcej reguł walidacji (cyfra, znak specjalny itd.)

            try
            {
                // Pobierz adres Supabase i klucz API z konfiguracji przez IOptions
                var supabaseUrl = _supabaseSettings.Url ?? throw new Exception("Brak SUPABASE_URL w konfiguracji.");
                var supabaseApiKey = _supabaseSettings.Key ?? throw new Exception("Brak SUPABASE_API_KEY w konfiguracji.");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("apikey", supabaseApiKey);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseApiKey}");

                var requestBody = new
                {
                    token = resetPasswordDto.Token,
                    type = "recovery",
                    password = resetPasswordDto.NewPassword
                };
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{supabaseUrl}/auth/v1/verify", content);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Błąd resetowania hasła przez Supabase: {Error}", error);
                    throw new Exception("Nie udało się zresetować hasła. Sprawdź ważność linku resetującego.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd resetowania hasła przez Supabase.");
                throw new Exception("Wystąpił błąd podczas resetowania hasła. Spróbuj ponownie później.");
            }
        }
    }
}
