using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MindfulDigger.Models;
using Supabase;

namespace MindfulDigger.Services
{
    public class SummaryService : ISummaryService
    {
        private readonly ISqlClintFactory _supabaseClientFactory;
        private readonly ILogger<SummaryService> _logger;

        public SummaryService(ISqlClintFactory supabaseClientFactory, ILogger<SummaryService> logger)
        {
            _supabaseClientFactory = supabaseClientFactory ?? throw new ArgumentNullException(nameof(supabaseClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Summary?> GetSummaryByIdAsync(Guid summaryId)
        {
            if (summaryId == Guid.Empty)
            {
                _logger.LogWarning("GetSummaryByIdAsync called with empty summaryId.");
                return null;
            }

            try
            {
                var supabase = await _supabaseClientFactory.CreateClient();
                var response = await supabase.From<Summary>()
                                             .Where(s => s.Id == summaryId)
                                             .Get();

                var summary = response.Models.FirstOrDefault();

                if (summary == null)
                {
                    _logger.LogInformation("Summary with ID {SummaryId} not found.", summaryId);
                }
                else
                {
                    _logger.LogInformation("Summary with ID {SummaryId} found.", summaryId);
                }
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving summary with ID {SummaryId} from Supabase.", summaryId);
                // Propagate the exception or handle it as per application's error handling strategy
                // For now, returning null to indicate failure, consistent with "not found"
                return null;
            }
        }
    }
}
