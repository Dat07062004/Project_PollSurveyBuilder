using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PollSurveyBuilder.Application.DTOs;

namespace PollSurveyBuilder.Application.IServices;

public interface IQnAService
{
    Task<QnADto> SubmitQuestionAsync(string code, CreateQnARequest dto, Guid? userId = null);
    Task<List<QnADto>> GetQuestionsAsync(string pollCode);
    Task<bool> UpvoteQuestionAsync(Guid questionId);
    Task<bool> TogglePinAsync(Guid questionId);
}
