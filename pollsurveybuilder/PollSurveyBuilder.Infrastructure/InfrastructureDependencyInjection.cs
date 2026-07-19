using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PollSurveyBuilder.Application.IRepositories;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Infrastructure.DbContexts;
using PollSurveyBuilder.Infrastructure.Repositories;
using PollSurveyBuilder.Infrastructure.Services;

namespace PollSurveyBuilder.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var dbProvider = configuration["DbProvider"] ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=PollSurveyDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        services.AddDbContext<PollDbContext>(options =>
        {
            if (dbProvider.Equals("MySQL", StringComparison.OrdinalIgnoreCase) || connectionString.Contains("Port=") || connectionString.Contains("Uid="))
            {
                options.UseMySQL(connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        // Repositories registration
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPollRepository, PollRepository>();
        services.AddScoped<IVoteRepository, VoteRepository>();
        services.AddScoped<IQnARepository, QnARepository>();

        // Redis Cache setup
        var redisConn = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConn))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConn;
                options.InstanceName = "PollSurvey_";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();

        return services;
    }
}
