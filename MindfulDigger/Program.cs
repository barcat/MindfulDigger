using MindfulDigger.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
Env.Load();

// Pobierz zmienne środowiskowe tylko raz
var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? throw new InvalidOperationException("SUPABASE_URL environment variable is not configured.");
var supabaseAnonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? throw new InvalidOperationException("SUPABASE_ANON_KEY environment variable is not configured.");
var supabaseJwtSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET") ?? throw new InvalidOperationException("SUPABASE_JWT_SECRET environment variable is not configured.");

// Configure Supabase settings
builder.Services.Configure<SupabaseSettings>(options =>
{
    options.Url = supabaseUrl;
    options.Key = supabaseAnonKey;
    options.JwtSecret = supabaseJwtSecret;
});

// Add Authentication Services for Supabase JWT and Cookie
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    // inne opcje według potrzeb
})
.AddJwtBearer(options =>
{
    options.Authority = $"{supabaseUrl}/auth/v1";
    options.Audience = "authenticated";
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = options.Authority,
        ValidateAudience = true,
        ValidAudience = options.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabaseJwtSecret))
    };
});

builder.Services.AddRazorPages();

// Register the factory
builder.Services.AddSingleton<ISqlClientFactory, SupabaseClientFactory>();

// Register custom services
builder.Services.AddScoped<INoteService, NoteService>(); // Register NoteService
builder.Services.AddScoped<ISummaryService, SummaryService>(); // Register SummaryService
builder.Services.AddScoped<ILlmService, MockLlmService>(); // Register MockLlmService
builder.Services.AddScoped<IFeedbackService, FeedbackService>(); // Register FeedbackService
builder.Services.AddScoped<IAuthService, AuthService>(); // Dodaj tę linię

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Authentication middleware BEFORE Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Map API controllers
app.MapControllers(); // Add this line to map attribute-routed controllers like NotesController

app.Run();
