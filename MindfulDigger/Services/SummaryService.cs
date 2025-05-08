using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MindfulDigger.Models;
using Supabase;
using MindfulDigger.DTOs;

namespace MindfulDigger.Services
{
    public class SummaryService : ISummaryService
    {
        private readonly SupabaseClientFactory _supabaseClientFactory; // Corrected type name from previous edits if necessary, ensure it's this
        private readonly ILogger<SummaryService> _logger;

        public SummaryService(SupabaseClientFactory supabaseClientFactory, ILogger<SummaryService> logger)
        {
            _supabaseClientFactory = supabaseClientFactory ?? throw new ArgumentNullException(nameof(supabaseClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // Removed direct _supabaseClient initialization: this._supabaseClient = supabaseClientFactory.CreateClient().Result;
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
                var response = await supabase
                                            .From<Summary>()
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

        public async Task<PaginatedResponse<SummaryListItemDto>> GetSummariesAsync(string userId, int page, int pageSize)
        {
            _logger.LogInformation("Attempting to retrieve summaries for User {UserId}, Page {Page}, PageSize {PageSize}", userId, page, pageSize);

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid) || userGuid == Guid.Empty)
            {
                _logger.LogWarning("GetSummariesAsync called with invalid userId: {UserId}", userId);
                // Consider throwing an ArgumentException or returning a specific error response
                // For now, returning an empty response as per current error handling in GetSummaryByIdAsync
                return new PaginatedResponse<SummaryListItemDto>
                {
                    Items = Enumerable.Empty<SummaryListItemDto>(),
                    Pagination = new PaginationMetadataDto
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        TotalPages = 0
                    }
                };
            }

            try
            {
                var supabase = await _supabaseClientFactory.CreateClient();

                // First, get the total count of summaries for the user for pagination metadata
                var countResponse = await supabase.From<Summary>()
                                                  .Where(s => s.UserId == userGuid)
                                                  .Count(Supabase.Postgrest.Constants.CountType.Exact);

                long totalCount = countResponse;

                if (totalCount == 0)
                {
                    _logger.LogInformation("No summaries found for User {UserId}", userId);
                    return new PaginatedResponse<SummaryListItemDto>
                    {
                        Items = Enumerable.Empty<SummaryListItemDto>(),
                        Pagination = new PaginationMetadataDto
                        {
                            CurrentPage = page,
                            PageSize = pageSize,
                            TotalCount = 0,
                            TotalPages = 0
                        }
                    };
                }

                int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                int itemsToSkip = (page - 1) * pageSize;

                var summariesResponse = await supabase.From<Summary>()
                                             .Where(s => s.UserId == userGuid)
                                             .Order("generation_date", Supabase.Postgrest.Constants.Ordering.Descending) // Corrected OrderBy to Order
                                             .Range(itemsToSkip, itemsToSkip + pageSize - 1)
                                             .Get();

                var summaryListItems = summariesResponse.Models.Select(s => new SummaryListItemDto
                {
                    Id = s.Id,
                    GenerationDate = s.GenerationDate.DateTime, // Assuming GenerationDate is DateTimeOffset
                    PeriodDescription = s.PeriodDescription,
                    IsAutomatic = s.IsAutomatic
                }).ToList();

                _logger.LogInformation("Successfully retrieved {Count} summaries for User {UserId}", summaryListItems.Count, userId);

                return new PaginatedResponse<SummaryListItemDto>
                {
                    Items = summaryListItems,
                    Pagination = new PaginationMetadataDto
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = (int)totalCount, // Plan specifies int, ensure this is acceptable for potential data loss
                        TotalPages = totalPages
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving summaries for User {UserId} from Supabase. Page: {Page}, PageSize: {PageSize}", userId, page, pageSize);
                // Propagate or handle as per application's strategy. 
                // For now, rethrowing to be caught by a global handler or controller.
                throw;
            }
        }

        public async Task<(object Dto, int StatusCode)> RequestSummaryGenerationAsync(string userId, GenerateSummaryRequestDto requestDto)
        {
            _logger.LogInformation("Summary generation requested for user {UserId} with period {Period}", userId, requestDto.Period);

            if (!Guid.TryParse(userId, out Guid userGuid) || userGuid == Guid.Empty)
            {
                _logger.LogWarning("Invalid UserId format: {UserId}", userId);
                return (new { Title = "Invalid user ID format.", Status = 400 }, 400);
            }

            var allowedPeriods = new List<string> { "last_7_days", "last_14_days", "last_30_days", "last_10_notes" };
            var requestedPeriod = requestDto.Period.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(requestedPeriod) || !allowedPeriods.Contains(requestedPeriod))
            {
                _logger.LogWarning("Invalid period specified: {Period}", requestDto.Period);
                return (new { Title = "Invalid period specified.", Errors = new { period = new[] { "Invalid value for period." } }, Status = 400 }, 400);
            }

            DateTimeOffset periodStart = DateTimeOffset.MinValue;
            // DateTimeOffset periodEnd = DateTimeOffset.UtcNow; // Initial broad scope declaration removed/commented
            string periodDescription = string.Empty;
            List<Note> notesForSummary = new List<Note>();

            var supabase = await _supabaseClientFactory.CreateClient(); // Get client instance

            try
            {
                if (requestedPeriod == "last_10_notes")
                {
                    _logger.LogInformation("Calculating period for 'last_10_notes' for user {UserId}", userId);
                    var notesResponse = await supabase.From<Note>()
                                                      .Where(n => n.UserId == userGuid)
                                                      .Order("created_at", Postgrest.Constants.Ordering.Descending)
                                                      .Limit(10)
                                                      .Get();
                    
                    if (notesResponse.Models != null && notesResponse.Models.Any())
                    {
                        notesForSummary = notesResponse.Models;
                        periodStart = notesForSummary.Last().CreatedAt; // Oldest of the fetched
                        DateTimeOffset periodEnd = notesForSummary.First().CreatedAt;   // Newest of the fetched - Scoped here
                        periodDescription = $"Last {notesForSummary.Count} notes";
                        _logger.LogInformation("Found {NoteCount} notes for 'last_10_notes'. Period: {PeriodStart} to {PeriodEnd}", notesForSummary.Count, periodStart, periodEnd);
                    }
                    else
                    {
                        _logger.LogInformation("No notes found for user {UserId} to process 'last_10_notes'", userId);
                        return (new { Title = "No notes found for the user to summarize for 'last_10_notes'." }, 400);
                    }
                }
                else // Date-based periods
                {
                    DateTimeOffset now = DateTimeOffset.UtcNow; // Capture current time once for consistent range
                    DateTimeOffset periodEnd = now; // Set periodEnd for this specific calculation using the captured 'now'

                    switch (requestedPeriod)
                    {
                        case "last_7_days":
                            periodStart = now.AddDays(-7); // Use the same 'now'
                            periodDescription = "Last 7 days";
                            break;
                        case "last_14_days":
                            periodStart = now.AddDays(-14); // Use the same 'now'
                            periodDescription = "Last 14 days";
                            break;
                        case "last_30_days":
                            periodStart = now.AddDays(-30); // Use the same 'now'
                            periodDescription = "Last 30 days";
                            break;
                    }
                    _logger.LogInformation("Calculated period for {RequestedPeriod}: {PeriodStart} to {PeriodEnd}", requestedPeriod, periodStart, periodEnd);

                    // Check for note existence in this period
                    var notesInPeriodResponse = await supabase.From<Note>()
                                                              .Where(n => n.UserId == userGuid && n.CreatedAt >= periodStart && n.CreatedAt <= periodEnd) // periodEnd is now precisely defined
                                                              .Get(); // Get the notes themselves
                    
                    if (notesInPeriodResponse.Models != null && notesInPeriodResponse.Models.Any())
                    {
                        notesForSummary = notesInPeriodResponse.Models;
                        _logger.LogInformation("Found {NoteCount} notes in the period '{PeriodDescription}'", notesForSummary.Count, periodDescription);
                    }
                    else
                    {
                        _logger.LogInformation("No notes found for user {UserId} in the period '{PeriodDescription}'", userId, periodDescription);
                        return (new { Title = $"No notes found in the period: {periodDescription}." }, 400);
                    }
                }

                // If we reached here, notesForSummary contains the notes to be summarized.
                // The next step will be to create the Summary entity and then call LLM.

                // Placeholder for creating summary record and LLM call (from previous logic)
                var newSummary = new Summary // This object creation will be refined in next steps
                {
                    Id = Guid.NewGuid(),
                    UserId = userGuid,
                    PeriodDescription = periodDescription,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    IsAutomatic = false,
                    GenerationDate = DateTimeOffset.UtcNow,
                    Status = "Pending", // Will be updated after LLM
                    CreatedAt = DateTime.UtcNow,
                    Content = "Simulated summary content." // Placeholder, LLM will fill this
                };
                newSummary.Status = "Completed"; // Simulate completion for now

                // Actual DB insert of newSummary will be in the next step.
                // For now, just logging and returning simulated success.
                _logger.LogInformation("Proceeding with summary generation logic for {UserId}, period {PeriodDescription}. Notes count: {NoteCount}", userId, periodDescription, notesForSummary.Count);

                var summaryDetails = new SummaryDetailsDto
                {
                    Id = newSummary.Id,
                    UserId = newSummary.UserId,
                    Content = newSummary.Content,
                    GenerationDate = newSummary.GenerationDate,
                    PeriodDescription = newSummary.PeriodDescription,
                    PeriodStart = newSummary.PeriodStart,
                    PeriodEnd = newSummary.PeriodEnd,
                    IsAutomatic = newSummary.IsAutomatic,
                    CreatedAt = newSummary.CreatedAt,
                    Status = newSummary.Status
                };
                return (summaryDetails, 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during summary generation request for user {UserId}, period {Period}", userId, requestDto.Period);
                return (new { Title = "An unexpected error occurred while processing your request.", Status = 500 }, 500);
            }
        }
    }
}
