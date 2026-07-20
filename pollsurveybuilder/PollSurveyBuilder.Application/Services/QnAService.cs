using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IRepositories;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Application.Services;

public class QnAService : IQnAService
{
    private readonly IPollRepository _pollRepository;
    private readonly IQnARepository _qnaRepository;
    private readonly IRealtimeNotificationService _notificationService;

    public QnAService(
        IPollRepository pollRepository,
        IQnARepository qnaRepository,
        IRealtimeNotificationService notificationService)
    {
        _pollRepository = pollRepository;
        _qnaRepository = qnaRepository;
        _notificationService = notificationService;
    }

    public async Task<QnADto> SubmitQuestionAsync(string code, CreateQnARequest dto, Guid? userId = null)
    {
        var poll = await _pollRepository.GetByCodeAsync(code);
        if (poll == null) throw new KeyNotFoundException("Poll not found.");

        var question = new QnAQuestion
        {
            PollId = poll.Id,
            QuestionText = dto.QuestionText.Trim(),
            VoterToken = dto.VoterToken ?? Guid.NewGuid().ToString(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _qnaRepository.AddAsync(question);
        await _qnaRepository.SaveChangesAsync();

        var qnaDto = MapToDto(question);
        await _notificationService.NotifyQuestionAddedAsync(code, qnaDto);

        return qnaDto;
    }

    public async Task<List<QnADto>> GetQuestionsAsync(string pollCode)
    {
        var poll = await _pollRepository.GetByCodeAsync(pollCode);
        if (poll == null) throw new KeyNotFoundException("Poll not found.");

        var questions = await _qnaRepository.GetQuestionsByPollIdAsync(poll.Id);
        return questions.Select(MapToDto).ToList();
    }

    public async Task<bool> UpvoteQuestionAsync(Guid questionId)
    {
        var question = await _qnaRepository.GetByIdAsync(questionId);
        if (question == null) return false;

        question.Upvotes++;
        await _qnaRepository.UpdateAsync(question);
        await _qnaRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> TogglePinAsync(Guid questionId)
    {
        var question = await _qnaRepository.GetByIdAsync(questionId);
        if (question == null) return false;

        question.IsPinned = !question.IsPinned;
        await _qnaRepository.UpdateAsync(question);
        await _qnaRepository.SaveChangesAsync();

        return true;
    }

    private static QnADto MapToDto(QnAQuestion q) => new QnADto
    {
        Id = q.Id,
        QuestionText = q.QuestionText,
        Upvotes = q.Upvotes,
        IsPinned = q.IsPinned,
        CreatedAt = q.CreatedAt
    };
}
