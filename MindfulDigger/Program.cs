using MindfulDigger.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? throw new InvalidOperationException("SUPABASE_URL environment variable is not configured.");
var supabaseAnonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? throw new InvalidOperationException("SUPABASE_ANON_KEY environment variable is not configured.");
var supabaseJwtSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET") ?? throw new InvalidOperationException("SUPABASE_JWT_SECRET environment variable is not configured.");

builder.Services.Configure<SupabaseSettings>(options =>
{
    options.Url = supabaseUrl;
    options.Key = supabaseAnonKey;
    options.JwtSecret = supabaseJwtSecret;
});

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

builder.Services.AddSingleton<ISqlClientFactory, SupabaseClientFactory>();

builder.Services.AddScoped<INoteRepository, NoteRepository>(); // Dodaj tę linię
builder.Services.AddScoped<INoteService, NoteService>(); // Register NoteService
builder.Services.AddScoped<ISummaryService, SummaryService>(); // Register SummaryService
builder.Services.AddScoped<ILlmService, MockLlmService>(); // Register MockLlmService
builder.Services.AddScoped<IFeedbackService, FeedbackService>(); // Register FeedbackService
builder.Services.AddScoped<IAuthService, AuthService>(); // Dodaj tę linię

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

app.Run();
