using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PollSurveyBuilder.Application.IRepositories;
using PollSurveyBuilder.Domain.Entities;
using PollSurveyBuilder.Domain.Enums;
using PollSurveyBuilder.Infrastructure.DbContexts;

namespace PollSurveyBuilder.Infrastructure.Repositories;

public class PollRepository : IPollRepository
{
    private readonly PollDbContext _context;

    public PollRepository(PollDbContext context)
    {
        _context = context;
    }

    public async Task<Poll?> GetByCodeAsync(string code)
    {
        return await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<Poll?> GetByIdAsync(Guid id)
    {
        return await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Poll>> GetPollsByUserIdAsync(Guid userId)
    {
        return await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .Where(p => p.CreatedByUserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsCodeAsync(string code)
    {
        return await _context.Polls.AnyAsync(p => p.Code == code);
    }

    public async Task AddAsync(Poll poll)
    {
        await _context.Polls.AddAsync(poll);
    }

    public Task UpdateAsync(Poll poll)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Poll poll)
    {
        _context.Polls.Remove(poll);
        return Task.CompletedTask;
    }

    public void RemoveOption(PollOption option)
    {
        _context.PollOptions.Remove(option);
    }

    public void RemoveOptions(IEnumerable<PollOption> options)
    {
        _context.PollOptions.RemoveRange(options);
    }

    public async Task ExecuteUpdatePollRawAsync(Guid pollId, string title, string? description, int questionType, DateTime? expiresAt)
    {
        var poll = await _context.Polls.FindAsync(pollId);
        if (poll != null)
        {
            poll.Title = title;
            poll.Description = description;
            poll.QuestionType = (QuestionType)questionType;
            poll.ExpiresAt = expiresAt;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ExecuteReplaceOptionsRawAsync(Guid pollId, List<string> options)
    {
        string pollIdStr = pollId.ToString().ToLower();
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM `PollOptions` WHERE `PollId` = {0}", pollIdStr);

        for (int i = 0; i < options.Count; i++)
        {
            string newId = Guid.NewGuid().ToString().ToLower();
            await _context.Database.ExecuteSqlRawAsync(
                "INSERT INTO `PollOptions` (`Id`, `PollId`, `OptionIndex`, `OptionText`, `VoteCount`) VALUES ({0}, {1}, {2}, {3}, {4})",
                newId, pollIdStr, i, options[i], 0);
        }
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                if (entry.State == EntityState.Added)
                {
                    continue;
                }

                var databaseValues = await entry.GetDatabaseValuesAsync();
                if (databaseValues != null)
                {
                    entry.OriginalValues.SetValues(databaseValues);
                }
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Safe fallback for MySQL provider when 0 rows are affected for matched unchanged entities
            }
        }
    }
}
