// Filepath: d:\git\MindfulDigger\MindfulDigger\Services\ILlmService.cs
using MindfulDigger.Models;

namespace MindfulDigger.Services
{
    public interface ILlmService
    {
        Task<string> GenerateSummaryAsync(IEnumerable<Note> notes, string periodDescription);
    }
}