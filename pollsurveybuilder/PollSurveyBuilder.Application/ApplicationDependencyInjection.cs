using Microsoft.Extensions.DependencyInjection;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Application.Services;

namespace PollSurveyBuilder.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<IQnAService, QnAService>();

        return services;
    }
}
