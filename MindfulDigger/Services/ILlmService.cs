using MindfulDigger.Model;

namespace MindfulDigger.Services
{
    public interface ILlmService
    {
        Task<string> GenerateSummaryAsync(IEnumerable<Note> notes, string periodDescription);
    }
}