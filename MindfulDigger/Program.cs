using Supabase;
using MindfulDigger.Services;
using DotNetEnv;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Add this
using Microsoft.IdentityModel.Tokens; // Add this
using System.Text; // Add this

var builder = WebApplication.CreateBuilder(args);

// Load .env file
Env.Load();

// Configure Supabase settings
builder.Services.Configure<SupabaseSettings>(options =>
{
    options.Url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? throw new InvalidOperationException("SUPABASE_URL environment variable is not configured.");
    options.Key = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? throw new InvalidOperationException("SUPABASE_ANON_KEY environment variable is not configured.");
    // Add JWT Secret configuration
    options.JwtSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET") ?? throw new InvalidOperationException("SUPABASE_JWT_SECRET environment variable is not configured.");
});

// Add Authentication Services for Supabase JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var supabaseSettings = builder.Configuration.GetSection("Supabase").Get<SupabaseSettings>() ?? 
                               throw new InvalidOperationException("Supabase settings are not configured correctly.");
        var supabaseUrl = supabaseSettings.Url;
        var supabaseJwtSecret = supabaseSettings.JwtSecret;

        options.Authority = $"{supabaseUrl}/auth/v1"; // Standard Supabase Auth endpoint
        options.Audience = "authenticated"; // Default Supabase audience

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
