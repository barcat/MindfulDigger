using Supabase;
using MindfulDigger.Services;
using DotNetEnv; // Add this using statement
using Microsoft.Extensions.Options; // Add this using statement

var builder = WebApplication.CreateBuilder(args);

// Load .env file
Env.Load(); // Add this line

// Configure Supabase settings
builder.Services.Configure<SupabaseSettings>(options =>
{
    options.Url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? throw new InvalidOperationException("SUPABASE_URL environment variable is not configured.");
    options.Key = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? throw new InvalidOperationException("SUPABASE_ANON_KEY environment variable is not configured.");
});

builder.Services.AddRazorPages();

// Register the factory
builder.Services.AddSingleton<ISqlClintFactory, SupabaseClientFactory>();

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
