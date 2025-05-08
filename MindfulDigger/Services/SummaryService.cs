using MindfulDigger.Models;
using MindfulDigger.DTOs;

namespace MindfulDigger.Services
{
    public class SummaryService : ISummaryService
    {
        private readonly SupabaseClientFactory _supabaseClientFactory; // Corrected type name from previous edits if necessary, ensure it's this
        private readonly ILogger<SummaryService> _logger;
        private readonly ILlmService _llmService; // Added ILlmService

        public SummaryService(SupabaseClientFactory supabaseClientFactory, ILogger<SummaryService> logger, ILlmService llmService) // Added ILlmService
        {
            _supabaseClientFactory = supabaseClientFactory ?? throw new ArgumentNullException(nameof(supabaseClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService)); // Added ILlmService
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

        // Renamed from RequestSummaryGenerationAsync and updated signature
        public async Task<(SummaryDetailsDto Dto, int StatusCode)> GenerateSummaryAsync(string userId, GenerateSummaryRequestDto requestDto)
        {
            _logger.LogInformation("Summary generation requested for user {UserId} with period {Period}", userId, requestDto.Period);

            if (!Guid.TryParse(userId, out Guid userGuid) || userGuid == Guid.Empty)
            {
                _logger.LogWarning("Invalid UserId format: {UserId}", userId);
                // Returning a placeholder DTO for error, actual error DTO structure might differ
                return (new SummaryDetailsDto { Content = "Invalid user ID format." }, StatusCodes.Status400BadRequest);
            }

            var allowedPeriods = new List<string> { "last_7_days", "last_14_days", "last_30_days", "last_10_notes" };
            var requestedPeriod = requestDto.Period.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(requestedPeriod) || !allowedPeriods.Contains(requestedPeriod))
            {
                _logger.LogWarning("Invalid period specified: {Period}", requestDto.Period);
                // Returning a placeholder DTO for error
                return (new SummaryDetailsDto { Content = $"Invalid value for period: {requestDto.Period}." }, StatusCodes.Status400BadRequest);
            }

            DateTimeOffset periodStart = DateTimeOffset.MinValue;
            DateTimeOffset periodEnd = DateTimeOffset.UtcNow; // Default end to now, will be refined
            string periodDescription = string.Empty;
            List<Note> notesForSummary = new List<Note>();

            var supabase = await _supabaseClientFactory.CreateClient();

            try
            {
                if (requestedPeriod == "last_10_notes")
                {
                    _logger.LogInformation("Calculating period for 'last_10_notes' for user {UserId}", userId);
                    var notesResponse = await supabase.From<Note>()
                                                      .Where(n => n.UserId == userGuid)
                                                      .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                                                      .Limit(10)
                                                      .Get();
                    
                    if (notesResponse.Models != null && notesResponse.Models.Any())
                    {
                        notesForSummary = notesResponse.Models;
                        periodStart = notesForSummary.Last().CreationDate; 
                        periodEnd = notesForSummary.First().CreationDate; 
                        periodDescription = $"Last {notesForSummary.Count} notes";
                        _logger.LogInformation("Found {NoteCount} notes for 'last_10_notes'. Period: {PeriodStart} to {PeriodEnd}", notesForSummary.Count, periodStart, periodEnd);
                    }
                    else
                    {
                        _logger.LogInformation("No notes found for user {UserId} to process 'last_10_notes'", userId);
                        return (new SummaryDetailsDto { Content = "No notes found for the user to summarize for 'last_10_notes'." }, StatusCodes.Status400BadRequest);
                    }
                }
                else 
                {
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    periodEnd = now; 

                    switch (requestedPeriod)
                    {
                        case "last_7_days":
                            periodStart = now.AddDays(-7);
                            periodDescription = "Last 7 days";
                            break;
                        case "last_14_days":
                            periodStart = now.AddDays(-14);
                            periodDescription = "Last 14 days";
                            break;
                        case "last_30_days":
                            periodStart = now.AddDays(-30);
                            periodDescription = "Last 30 days";
                            break;
                    }
                    _logger.LogInformation("Calculated period for {RequestedPeriod}: {PeriodStart} to {PeriodEnd}", requestedPeriod, periodStart, periodEnd);

                    var notesInPeriodResponse = await supabase.From<Note>()
                                                              .Where(n => n.UserId == userGuid && n.CreationDate >= periodStart && n.CreationDate <= periodEnd)
                                                              .Get(); 
                    
                    if (notesInPeriodResponse.Models != null && notesInPeriodResponse.Models.Any())
                    {
                        notesForSummary = notesInPeriodResponse.Models;
                        _logger.LogInformation("Found {NoteCount} notes in the period '{PeriodDescription}'", notesForSummary.Count, periodDescription);
                    }
                    else
                    {
                        _logger.LogInformation("No notes found for user {UserId} in the period '{PeriodDescription}'", userId, periodDescription);
                        return (new SummaryDetailsDto { Content = $"No notes found in the period: {periodDescription}." }, StatusCodes.Status400BadRequest);
                    }
                }

                // Generate summary content using LLM Service
                string summaryContent = await _llmService.GenerateSummaryAsync(notesForSummary, periodDescription);

                var newSummary = new Summary
                {
                    Id = Guid.NewGuid(),
                    UserId = userGuid,
                    Content = summaryContent,
                    GenerationDate = DateTimeOffset.UtcNow,
                    PeriodDescription = periodDescription,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    IsAutomatic = false, 
                    Status = "Completed", // Synchronous, so completed
                    CreatedAt = DateTime.UtcNow 
                };

                var insertResponse = await supabase.From<Summary>().Insert(newSummary);
                var createdSummary = insertResponse.Models.FirstOrDefault();

                if (createdSummary == null)
                {
                    _logger.LogError("Failed to save the new summary for user {UserId}, period {PeriodDescription}", userId, periodDescription);
                    return (new SummaryDetailsDto { Content = "Failed to save summary." }, StatusCodes.Status500InternalServerError);
                }

                _logger.LogInformation("Successfully generated and saved summary {SummaryId} for user {UserId}, period {PeriodDescription}", createdSummary.Id, userId, periodDescription);

                var summaryDetailsDto = new SummaryDetailsDto
                {
                    Id = createdSummary.Id,
                    UserId = createdSummary.UserId,
                    Content = createdSummary.Content,
                    GenerationDate = createdSummary.GenerationDate,
                    PeriodDescription = createdSummary.PeriodDescription,
                    PeriodStart = createdSummary.PeriodStart,
                    PeriodEnd = createdSummary.PeriodEnd,
                    IsAutomatic = createdSummary.IsAutomatic,
                    CreatedAt = createdSummary.CreatedAt,
                    Status = createdSummary.Status
                };
                return (summaryDetailsDto, StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during summary generation for user {UserId}, period {Period}", userId, requestDto.Period);
                return (new SummaryDetailsDto { Content = "An unexpected error occurred while processing your request." }, StatusCodes.Status500InternalServerError);
            }
        }
    }
}
