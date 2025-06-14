using MindfulDigger.Data.Supabase;
using MindfulDigger.Model;

namespace MindfulDigger.Services
{
    public class SummaryService : ISummaryService
    {
        private readonly ISummaryRepository _summaryRepository;
        private readonly ILogger<SummaryService> _logger;
        private readonly ILlmService _llmService;

        public SummaryService(ISummaryRepository summaryRepository, ILogger<SummaryService> logger, ILlmService llmService)
        {
            _summaryRepository = summaryRepository ?? throw new ArgumentNullException(nameof(summaryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
        }

        public async Task<Summary?> GetSummaryByIdAsync(Guid summaryId, string jwt, string refreshToken)
        {
            if (summaryId == Guid.Empty)
            {
                _logger.LogWarning("GetSummaryByIdAsync called with empty summaryId.");
                return null;
            }
            try
            {
                var summary = await _summaryRepository.GetSummaryByIdAsync(summaryId, jwt, refreshToken);

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
                _logger.LogError(ex, "Error retrieving summary with ID {SummaryId} from repository.", summaryId);
                return null;
            }
        }

        public async Task<PaginatedResponse<SummaryListItemDto>> GetSummariesAsync(string userId, int page, int pageSize, string jwt, string refreshToken)
        {
            _logger.LogInformation("Attempting to retrieve summaries for User {UserId}, Page {Page}, PageSize {PageSize}", userId, page, pageSize);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid) || userGuid == Guid.Empty)
            {
                _logger.LogWarning("GetSummariesAsync called with invalid userId: {UserId}", userId);
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
                var (summaries, totalCount) = await _summaryRepository.GetUserSummariesPaginatedAsync(userGuid, page, pageSize, jwt, refreshToken);
                int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var summaryListItems = summaries.Select(s => new SummaryListItemDto
                {
                    Id = s.Id,
                    GenerationDate = s.GenerationDate.DateTime,
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
                        TotalCount = (int)totalCount,
                        TotalPages = totalPages
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving summaries for User {UserId} from repository. Page: {Page}, PageSize: {PageSize}", userId, page, pageSize);
                throw;
            }
        }

        public async Task<(SummaryDetailsDto Dto, int StatusCode)> GenerateSummaryAsync(string userId, GenerateSummaryRequestDto requestDto, string jwt, string refreshToken)
        {
            _logger.LogInformation("Summary generation requested for user {UserId} with period {Period}", userId, requestDto.Period);
            if (!Guid.TryParse(userId, out Guid userGuid) || userGuid == Guid.Empty)
            {
                _logger.LogWarning("Invalid UserId format: {UserId}", userId);
                return (new SummaryDetailsDto { Content = "Invalid user ID format." }, StatusCodes.Status400BadRequest);
            }
            var allowedPeriods = new List<string> { "last_7_days", "last_14_days", "last_30_days", "last_10_notes" };
            if (string.IsNullOrWhiteSpace(requestDto.Period))
            {
                _logger.LogWarning("Period is null or whitespace.");
                return (new SummaryDetailsDto { Content = "Period cannot be null or whitespace." }, StatusCodes.Status400BadRequest);
            }
            var requestedPeriod = requestDto.Period.ToLowerInvariant();
            if (!allowedPeriods.Contains(requestedPeriod))
            {
                _logger.LogWarning("Invalid period specified: {Period}", requestDto.Period);
                return (new SummaryDetailsDto { Content = $"Invalid value for period: {requestDto.Period}." }, StatusCodes.Status400BadRequest);
            }
            DateTimeOffset periodStart = DateTimeOffset.MinValue;
            DateTimeOffset periodEnd = DateTimeOffset.UtcNow;
            string periodDescription = string.Empty;
            List<Note> notesForSummary = new List<Note>();
            try
            {
                if (requestedPeriod == "last_10_notes")
                {
                    _logger.LogInformation("Calculating period for 'last_10_notes' for user {UserId}", userId);
                    notesForSummary = await _summaryRepository.GetNotesForSummaryAsync(userGuid, requestedPeriod, null, null, jwt, refreshToken);
                    if (notesForSummary != null && notesForSummary.Any())
                    {
                        periodStart = notesForSummary.Last()!.CreationDate;
                        periodEnd = notesForSummary.First()!.CreationDate;
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

                    notesForSummary = await _summaryRepository.GetNotesForSummaryAsync(userGuid, requestedPeriod, periodStart, periodEnd, jwt, refreshToken);

                    if (notesForSummary != null && notesForSummary.Any())
                    {
                        _logger.LogInformation("Found {NoteCount} notes in the period '{PeriodDescription}'", notesForSummary.Count, periodDescription);
                    }
                    else
                    {
                        _logger.LogInformation("No notes found for user {UserId} in the period '{PeriodDescription}'", userId, periodDescription);
                        return (new SummaryDetailsDto { Content = $"No notes found in the period: {periodDescription}." }, StatusCodes.Status400BadRequest);
                    }
                }
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
                    Status = "Completed",
                };
                var createdSummary = await _summaryRepository.InsertSummaryAsync(newSummary, jwt, refreshToken);
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
