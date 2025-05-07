using System;
using System.Threading.Tasks;
using MindfulDigger.Models;

namespace MindfulDigger.Services
{
    public interface ISummaryService
    {
        Task<Summary?> GetSummaryByIdAsync(Guid summaryId);
    }
}