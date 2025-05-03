using Supabase;
using System;
using Microsoft.Extensions.Options; // Add this using statement
using System.Threading.Tasks; // Add this using statement

namespace MindfulDigger.Services
{
    public class SupabaseClientFactory : ISqlClintFactory
    {
        private readonly SupabaseSettings _settings;

        public SupabaseClientFactory(IOptions<SupabaseSettings> settings)
        {
            _settings = settings.Value;

            if (string.IsNullOrEmpty(_settings.Url) || string.IsNullOrEmpty(_settings.Key))
            {
                throw new InvalidOperationException("Supabase URL and Key must be configured.");
            }
        }

        public async Task<Client> CreateClient()
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
            };

            var client = new Client(_settings.Url, _settings.Key, options);
            await client.InitializeAsync();

            return client;
        }
    }

    public interface ISqlClintFactory
    {
        Task<Client> CreateClient();
    }
}