using System;
using System.Threading.Tasks;
using PollSurveyBuilder.Application.DTOs;

namespace PollSurveyBuilder.Application.IServices;

public interface IPollService
{
    Task<PollDetailsDto> CreatePollAsync(CreatePollRequest dto, Guid? userId = null);
    Task<PollDetailsDto?> GetPollByCodeAsync(string code);
    Task<PollDetailsDto> UpdatePollAsync(string code, UpdatePollRequest dto, Guid? userId = null);
    Task<bool> DeletePollAsync(string code, Guid? userId = null);
    Task<bool> ClosePollAsync(string code, Guid? userId = null);
}
