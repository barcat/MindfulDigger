// Filepath: d:\git\MindfulDigger\MindfulDigger\Services\MockLlmService.cs
using MindfulDigger.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindfulDigger.Services
{
    public class MockLlmService : ILlmService
    {
        public Task<string> GenerateSummaryAsync(IEnumerable<Note> notes, string periodDescription)
        {
            if (notes == null || !notes.Any())
            {
                return Task.FromResult(
                    $"## Podsumowanie dla okresu: {periodDescription}\n\n" +
                    $"### Brak notatek\n" +
                    $"Nie znaleziono notatek w wybranym okresie do wygenerowania podsumowania."
                );
            }

            return Task.FromResult(
                $"## Podsumowanie dla okresu: {periodDescription}\n\n" +
                $"### Główne tematy\n" +
                $"- Temat 1 (na podstawie notatek)\n- Temat 2 (na podstawie notatek)\n- Temat 3 (na podstawie notatek)\n\n" +
                $"### Nastrój\n" +
                $"Ogólnie pozytywny (analiza z notatek)\n\n" +
                $"### Wnioski\n" +
                $"To jest przykładowe podsumowanie wygenerowane przez mockowy LLM na podstawie dostarczonych notatek."
            );
        }
    }
}