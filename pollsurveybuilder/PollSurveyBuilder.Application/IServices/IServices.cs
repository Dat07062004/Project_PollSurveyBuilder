using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PollSurveyBuilder.Application.DTOs;

namespace PollSurveyBuilder.Application.IServices;

public interface IPollService
{
    Task<PollDetailsDto> CreatePollAsync(CreatePollRequest dto, Guid? userId = null);
    Task<PollDetailsDto?> GetPollByCodeAsync(string code);
    Task<bool> ClosePollAsync(string code, Guid? userId = null);
}

public interface IVoteService
{
    Task<PollResultsDto> CastVoteAsync(string code, SubmitVoteRequest dto, Guid? userId = null);
    Task<PollResultsDto> GetPollResultsAsync(string code);
    Task<PollAnalyticsDto> GetPollAnalyticsAsync(string code);
}

public interface IQnAService
{
    Task<QnADto> SubmitQuestionAsync(string code, CreateQnARequest dto, Guid? userId = null);
    Task<List<QnADto>> GetQuestionsAsync(string pollCode);
    Task<bool> UpvoteQuestionAsync(Guid questionId);
    Task<bool> TogglePinAsync(Guid questionId);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null);
    Task RemoveAsync(string key);
}

public interface IRealtimeNotificationService
{
    Task NotifyVoteUpdatedAsync(string pollCode, PollResultsDto results);
    Task NotifyQuestionAddedAsync(string pollCode, QnADto question);
}
