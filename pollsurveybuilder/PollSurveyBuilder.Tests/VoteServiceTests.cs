using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.Services;
using PollSurveyBuilder.Domain.Entities;
using PollSurveyBuilder.Domain.Enums;
using PollSurveyBuilder.Infrastructure.DbContexts;
using PollSurveyBuilder.Infrastructure.Repositories;
using Xunit;

namespace PollSurveyBuilder.Tests;

public class VoteServiceTests
{
    private PollDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<PollDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PollDbContext(options);
    }

    [Fact]
    public async Task CastVoteAsync_ShouldThrow_WhenPollIsClosed()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var voteRepo = new VoteRepository(db);
        var cacheService = new DummyCacheService();
        var notifService = new DummyNotificationService();
        var pollService = new PollService(pollRepo, cacheService);
        var voteService = new VoteService(pollRepo, voteRepo, cacheService, notifService);

        var poll = await pollService.CreatePollAsync(new CreatePollRequest
        {
            Title = "Closed Poll Test",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "Option A" }
        });

        await pollService.ClosePollAsync(poll.Code, null);

        // Act & Assert
        Func<Task> act = async () => await voteService.CastVoteAsync(poll.Code, new SubmitVoteRequest { OptionIndex = 0 }, null);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*closed*");
    }

    [Fact]
    public async Task CastVoteAsync_ShouldThrow_WhenPollIsExpired()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var voteRepo = new VoteRepository(db);
        var cacheService = new DummyCacheService();
        var notifService = new DummyNotificationService();
        var pollService = new PollService(pollRepo, cacheService);
        var voteService = new VoteService(pollRepo, voteRepo, cacheService, notifService);

        var expiredPoll = new Poll
        {
            Code = "EXPIRE",
            Title = "Expired Poll",
            QuestionType = QuestionType.MultipleChoice,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        await pollRepo.AddAsync(expiredPoll);
        await pollRepo.SaveChangesAsync();

        // Act & Assert
        Func<Task> act = async () => await voteService.CastVoteAsync("EXPIRE", new SubmitVoteRequest { OptionIndex = 0 }, null);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*expired*");
    }

    [Fact]
    public async Task CastVoteAsync_ShouldStoreOpenEndedTextResponse()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var voteRepo = new VoteRepository(db);
        var cacheService = new DummyCacheService();
        var notifService = new DummyNotificationService();
        var pollService = new PollService(pollRepo, cacheService);
        var voteService = new VoteService(pollRepo, voteRepo, cacheService, notifService);

        var poll = await pollService.CreatePollAsync(new CreatePollRequest
        {
            Title = "Feedback Poll",
            QuestionType = QuestionType.OpenText
        });

        // Act
        var results = await voteService.CastVoteAsync(poll.Code, new SubmitVoteRequest
        {
            OptionIndex = 0,
            TextResponse = "Great platform!",
            VoterToken = "voter_1"
        }, null);

        // Assert
        results.TotalVotes.Should().Be(1);
        results.OpenTextResponses.Should().Contain("Great platform!");
    }

    [Fact]
    public async Task GetPollResultsAsync_ShouldCalculatePercentagesAccurately()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var voteRepo = new VoteRepository(db);
        var cacheService = new DummyCacheService();
        var notifService = new DummyNotificationService();
        var pollService = new PollService(pollRepo, cacheService);
        var voteService = new VoteService(pollRepo, voteRepo, cacheService, notifService);

        var poll = await pollService.CreatePollAsync(new CreatePollRequest
        {
            Title = "Percentage Test Poll",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "Option A", "Option B" }
        });

        // 3 votes for Option 0, 1 vote for Option 1
        await voteService.CastVoteAsync(poll.Code, new SubmitVoteRequest { OptionIndex = 0, VoterToken = "v1" });
        await voteService.CastVoteAsync(poll.Code, new SubmitVoteRequest { OptionIndex = 0, VoterToken = "v2" });
        await voteService.CastVoteAsync(poll.Code, new SubmitVoteRequest { OptionIndex = 0, VoterToken = "v3" });
        await voteService.CastVoteAsync(poll.Code, new SubmitVoteRequest { OptionIndex = 1, VoterToken = "v4" });

        // Act
        var results = await voteService.GetPollResultsAsync(poll.Code);

        // Assert
        results.TotalVotes.Should().Be(4);
        results.Options[0].VoteCount.Should().Be(3);
        results.Options[0].Percentage.Should().Be(75.0);
        results.Options[1].VoteCount.Should().Be(1);
        results.Options[1].Percentage.Should().Be(25.0);
    }

    [Fact]
    public async Task GetPollAnalyticsAsync_ShouldReturnValidAnalytics()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var voteRepo = new VoteRepository(db);
        var cacheService = new DummyCacheService();
        var notifService = new DummyNotificationService();
        var pollService = new PollService(pollRepo, cacheService);
        var voteService = new VoteService(pollRepo, voteRepo, cacheService, notifService);

        var poll = await pollService.CreatePollAsync(new CreatePollRequest
        {
            Title = "Analytics Test Poll",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "Apple", "Banana" }
        });

        await voteService.CastVoteAsync(poll.Code, new SubmitVoteRequest { OptionIndex = 0, VoterToken = "v1" });
        await voteService.CastVoteAsync(poll.Code, new SubmitVoteRequest { OptionIndex = 0, VoterToken = "v2" });

        // Act
        var analytics = await voteService.GetPollAnalyticsAsync(poll.Code);

        // Assert
        analytics.Should().NotBeNull();
        analytics.TotalVotes.Should().Be(2);
        analytics.TopOption.Should().Be("Apple");
        analytics.VotesOverTime.Should().NotBeEmpty();
    }
}
