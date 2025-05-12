using MindfulDigger.DTOs;
using Supabase;
using Supabase.Gotrue.Exceptions; // Corrected namespace

namespace MindfulDigger.Services
{
    public class AuthService : IAuthService
    {
        private readonly ISqlClientFactory _clientFactory;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ISqlClientFactory clientFactory, ILogger<AuthService> logger)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    }
}
