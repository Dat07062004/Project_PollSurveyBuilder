using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PollSurveyBuilder.Application.IRepositories;
using PollSurveyBuilder.Domain.Entities;
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
        _context.Polls.Update(poll);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

public class VoteRepository : IVoteRepository
{
    private readonly PollDbContext _context;

    public VoteRepository(PollDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasVotedAsync(Guid pollId, string voterToken)
    {
        return await _context.Votes.AnyAsync(v => v.PollId == pollId && v.VoterToken == voterToken);
    }

    public async Task<bool> HasUserVotedAsync(Guid pollId, Guid userId)
    {
        return await _context.Votes.AnyAsync(v => v.PollId == pollId && v.UserId == userId);
    }

    public async Task AddAsync(Vote vote)
    {
        await _context.Votes.AddAsync(vote);
    }

    public async Task<List<Vote>> GetVotesByPollIdAsync(Guid pollId)
    {
        return await _context.Votes
            .Where(v => v.PollId == pollId)
            .OrderBy(v => v.VotedAt)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

public class QnARepository : IQnARepository
{
    private readonly PollDbContext _context;

    public QnARepository(PollDbContext context)
    {
        _context = context;
    }

    public async Task<QnAQuestion?> GetByIdAsync(Guid id)
    {
        return await _context.QnAQuestions.FindAsync(id);
    }

    public async Task<List<QnAQuestion>> GetQuestionsByPollIdAsync(Guid pollId)
    {
        return await _context.QnAQuestions
            .Where(q => q.PollId == pollId)
            .OrderByDescending(q => q.IsPinned)
            .ThenByDescending(q => q.Upvotes)
            .ThenByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(QnAQuestion question)
    {
        await _context.QnAQuestions.AddAsync(question);
    }

    public Task UpdateAsync(QnAQuestion question)
    {
        _context.QnAQuestions.Update(question);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
