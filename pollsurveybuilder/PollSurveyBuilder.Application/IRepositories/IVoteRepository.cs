using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Application.IRepositories;

public interface IVoteRepository
{
    Task<bool> HasVotedAsync(Guid pollId, string voterToken);
    Task<bool> HasUserVotedAsync(Guid pollId, Guid userId);
    Task AddAsync(Vote vote);
    Task<List<Vote>> GetVotesByPollIdAsync(Guid pollId);
    Task SaveChangesAsync();
}
