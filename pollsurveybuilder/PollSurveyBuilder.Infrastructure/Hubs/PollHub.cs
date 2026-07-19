using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace PollSurveyBuilder.Infrastructure.Hubs;

public class PollHub : Hub
{
    public async Task JoinPollGroup(string pollCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, pollCode);
    }

    public async Task LeavePollGroup(string pollCode)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, pollCode);
    }
}
