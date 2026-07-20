using System;
using System.Threading.Tasks;
using PollSurveyBuilder.Application.DTOs;

namespace PollSurveyBuilder.Application.IServices;

public interface IVoteService
{
    Task<PollResultsDto> CastVoteAsync(string code, SubmitVoteRequest dto, Guid? userId = null);
    Task<PollResultsDto> GetPollResultsAsync(string code);
    Task<PollAnalyticsDto> GetPollAnalyticsAsync(string code);
}
