using System.Threading.Tasks;
using PollSurveyBuilder.Application.DTOs;

namespace PollSurveyBuilder.Application.IServices;

public interface IRealtimeNotificationService
{
    Task NotifyVoteUpdatedAsync(string pollCode, PollResultsDto results);
    Task NotifyQuestionAddedAsync(string pollCode, QnADto question);
}
