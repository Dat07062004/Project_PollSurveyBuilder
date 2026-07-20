using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PollSurveyBuilder.Application.IRepositories;
using PollSurveyBuilder.Domain.Entities;
using PollSurveyBuilder.Infrastructure.DbContexts;

namespace PollSurveyBuilder.Infrastructure.Repositories;

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
