using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Application.IRepositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<bool> ExistsEmailAsync(string email);
    Task<bool> ExistsUsernameAsync(string username);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task SaveChangesAsync();
}

public interface IPollRepository
{
    Task<Poll?> GetByCodeAsync(string code);
    Task<Poll?> GetByIdAsync(Guid id);
    Task<List<Poll>> GetPollsByUserIdAsync(Guid userId);
    Task<bool> ExistsCodeAsync(string code);
    Task AddAsync(Poll poll);
    Task UpdateAsync(Poll poll);
    Task SaveChangesAsync();
}

public interface IVoteRepository
{
    Task<bool> HasVotedAsync(Guid pollId, string voterToken);
    Task<bool> HasUserVotedAsync(Guid pollId, Guid userId);
    Task AddAsync(Vote vote);
    Task<List<Vote>> GetVotesByPollIdAsync(Guid pollId);
    Task SaveChangesAsync();
}

public interface IQnARepository
{
    Task<QnAQuestion?> GetByIdAsync(Guid id);
    Task<List<QnAQuestion>> GetQuestionsByPollIdAsync(Guid pollId);
    Task AddAsync(QnAQuestion question);
    Task UpdateAsync(QnAQuestion question);
    Task SaveChangesAsync();
}
