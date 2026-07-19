using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Infrastructure.Hubs;

namespace PollSurveyBuilder.Infrastructure.Services;

public class SignalRNotificationService : IRealtimeNotificationService
{
    private readonly IHubContext<PollHub> _hubContext;

    public SignalRNotificationService(IHubContext<PollHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyVoteUpdatedAsync(string pollCode, PollResultsDto results)
    {
        await _hubContext.Clients.Group(pollCode).SendAsync("ReceiveVoteUpdate", results);
    }

    public async Task NotifyQuestionAddedAsync(string pollCode, QnADto question)
    {
        await _hubContext.Clients.Group(pollCode).SendAsync("ReceiveNewQuestion", question);
    }
}
