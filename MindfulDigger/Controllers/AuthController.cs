using Microsoft.AspNetCore.Mvc;
using MindfulDigger.Model;
using MindfulDigger.Services;
using Supabase.Gotrue.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace MindfulDigger.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger; // Logger instance

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (loginRequest == null)
            {
                _logger.LogWarning("Login request body is null.");
                return BadRequest("Login request cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login request model state is invalid.");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Attempting login for email: {Email}", loginRequest.Email);
                var (token, userId, refreshToken) = await _authService.LoginAsync(loginRequest);

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Login failed for email: {Email}. Invalid credentials.", loginRequest.Email);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Tworzenie claimów i ustawienie cookie autoryzacyjnego
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, loginRequest.Email),
                    new Claim("AccessToken", token), // Dodajemy token jako claim
                    new Claim("RefreshToken", refreshToken ?? string.Empty) // Dodajemy refresh token jako claim
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // lub false jeśli nie chcesz trwałego cookie
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                _logger.LogInformation("Login successful for email: {Email}", loginRequest.Email);
                // Zwracamy refreshToken w odpowiedzi
                return Ok(new LoginResponseDto { Token = token, UserId = userId, RefreshToken = refreshToken });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument exception during login for email: {Email}", loginRequest.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (GotrueException ex)
            {
                // Handle Supabase specific exceptions, e.g., invalid credentials
                _logger.LogWarning(ex, "Supabase authentication error during login for email: {Email}", loginRequest.Email);
                return Unauthorized(new { message = "Invalid email or password. " + ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login for email: {Email}", loginRequest.Email);
                // It's good practice to not expose raw exception details to the client.
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationDto registrationDto)
        {
            if (registrationDto == null)
                return BadRequest(new { message = "Brak danych rejestracyjnych." });
            try
            {
                await _authService.RegisterUser(registrationDto);
                return Ok(new { message = "Rejestracja zakończona sukcesem. Sprawdź e-mail w celu potwierdzenia." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (GotrueException ex)
            {
                return BadRequest(new { message = "Błąd rejestracji: " + ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Wystąpił nieoczekiwany błąd. Spróbuj ponownie później." });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (forgotPasswordDto == null)
                return BadRequest(new { message = "Brak danych." });
            try
            {
                await _authService.SendForgotPasswordEmail(forgotPasswordDto);
                return Ok(new { message = "Wysłano e-mail z linkiem do resetowania hasła." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (GotrueException ex)
            {
                return BadRequest(new { message = "Błąd wysyłki e-maila: " + ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Wystąpił nieoczekiwany błąd. Spróbuj ponownie później." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto == null)
                return BadRequest(new { message = "Brak danych." });
            try
            {
                await _authService.ResetPassword(resetPasswordDto);
                return Ok(new { message = "Hasło zostało zresetowane." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (GotrueException ex)
            {
                return BadRequest(new { message = "Błąd resetowania hasła: " + ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Wystąpił nieoczekiwany błąd. Spróbuj ponownie później." });
            }
        }
    }
}
