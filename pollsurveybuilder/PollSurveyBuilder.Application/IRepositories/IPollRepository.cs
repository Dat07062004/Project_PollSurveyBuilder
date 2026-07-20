using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Application.IRepositories;

public interface IPollRepository
{
    Task<Poll?> GetByCodeAsync(string code);
    Task<Poll?> GetByIdAsync(Guid id);
    Task<List<Poll>> GetPollsByUserIdAsync(Guid userId);
    Task<bool> ExistsCodeAsync(string code);
    Task AddAsync(Poll poll);
    Task UpdateAsync(Poll poll);
    Task DeleteAsync(Poll poll);
    void RemoveOption(PollOption option);
    void RemoveOptions(IEnumerable<PollOption> options);
    Task ExecuteUpdatePollRawAsync(Guid pollId, string title, string? description, int questionType, DateTime? expiresAt);
    Task ExecuteReplaceOptionsRawAsync(Guid pollId, List<string> options);
    Task SaveChangesAsync();
}
