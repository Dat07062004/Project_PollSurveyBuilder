using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Application.IRepositories;

public interface IQnARepository
{
    Task<QnAQuestion?> GetByIdAsync(Guid id);
    Task<List<QnAQuestion>> GetQuestionsByPollIdAsync(Guid pollId);
    Task AddAsync(QnAQuestion question);
    Task UpdateAsync(QnAQuestion question);
    Task SaveChangesAsync();
}
