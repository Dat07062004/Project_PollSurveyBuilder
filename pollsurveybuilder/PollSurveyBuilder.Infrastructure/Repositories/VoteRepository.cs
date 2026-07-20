using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PollSurveyBuilder.Application.IRepositories;
using PollSurveyBuilder.Domain.Entities;
using PollSurveyBuilder.Infrastructure.DbContexts;

namespace PollSurveyBuilder.Infrastructure.Repositories;

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
