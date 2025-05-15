using MindfulDigger.DTOs;
using Supabase;
using MindfulDigger.Services;

namespace MindfulDigger.Data.Supabase;

public interface IAuthRepository
{
    Task<(string? Token, string? UserId, string? RefreshToken)> LoginAsync(LoginRequestDto loginRequest);
    Task RegisterUserAsync(RegistrationDto registrationDto);
    Task SendForgotPasswordEmailAsync(ForgotPasswordDto forgotPasswordDto);
    Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto, SupabaseSettings supabaseSettings);
}

public class AuthRepository : IAuthRepository
{
    private readonly ISqlClientFactory _clientFactory;
    public AuthRepository(ISqlClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<(string? Token, string? UserId, string? RefreshToken)> LoginAsync(LoginRequestDto loginRequest)
    {
        var supabase = await _clientFactory.CreateClient();
        var session = await supabase.Auth.SignIn(loginRequest.Email, loginRequest.Password);
        if (session == null || string.IsNullOrEmpty(session.AccessToken) || session.User == null || string.IsNullOrEmpty(session.User.Id))
        {
            return (null, null, null);
        }
        return (session.AccessToken, session.User.Id, session.RefreshToken);
    }

    public async Task RegisterUserAsync(RegistrationDto registrationDto)
    {
        var supabase = await _clientFactory.CreateClient();
        var response = await supabase.Auth.SignUp(registrationDto.Email, registrationDto.Password);
        if (response == null || response.User == null)
            throw new Exception("Nie udało się utworzyć konta użytkownika.");
    }

    public async Task SendForgotPasswordEmailAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var supabase = await _clientFactory.CreateClient();
        await supabase.Auth.ResetPasswordForEmail(forgotPasswordDto.Email);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto, SupabaseSettings supabaseSettings)
    {
        var supabaseUrl = supabaseSettings.Url ?? throw new Exception("Brak SUPABASE_URL w konfiguracji.");
        var supabaseApiKey = supabaseSettings.Key ?? throw new Exception("Brak SUPABASE_API_KEY w konfiguracji.");
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
            throw new Exception($"Nie udało się zresetować hasła: {error}");
        }
    }
}
