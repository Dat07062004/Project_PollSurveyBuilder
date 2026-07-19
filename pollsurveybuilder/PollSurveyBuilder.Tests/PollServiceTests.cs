using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Application.Services;
using PollSurveyBuilder.Domain.Entities;
using PollSurveyBuilder.Domain.Enums;
using PollSurveyBuilder.Infrastructure.DbContexts;
using PollSurveyBuilder.Infrastructure.Repositories;
using Xunit;

namespace PollSurveyBuilder.Tests;

public class PollServiceTests
{
    private PollDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<PollDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PollDbContext(options);
    }

    [Fact]
    public async Task CreatePollAsync_WithValidMultipleChoice_ShouldCreatePollAndOptions()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);

        var service = new PollService(pollRepo, new DummyCacheService());

        var request = new CreatePollRequest
        {
            Title = "Favorite Programming Language?",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "C#", "TypeScript", "Python" }
        };

        // Act
        var result = await service.CreatePollAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Favorite Programming Language?");
        result.Code.Should().HaveLength(6);
        result.Options.Should().HaveCount(3);
        result.Options[0].OptionText.Should().Be("C#");
    }

    [Fact]
    public async Task SubmitVoteAsync_ShouldPreventDuplicateVotes_FromSameUser()
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
            Title = "Test Poll",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "Option A", "Option B" }
        });

        var userId = Guid.NewGuid();

        // Act & Assert
        var result1 = await voteService.CastVoteAsync(poll.Code, new SubmitVoteRequest { OptionIndex = 0 }, userId);
        result1.Should().NotBeNull();
        result1.TotalVotes.Should().Be(1);

        Func<Task> act = async () => await voteService.CastVoteAsync(poll.Code, new SubmitVoteRequest { OptionIndex = 1 }, userId);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*đã thực hiện bình chọn*");
    }
}

// Dummy Test Helpers
public class DummyCacheService : ICacheService
{
    private readonly Dictionary<string, object> _memory = new();

    public Task<T?> GetAsync<T>(string key)
    {
        if (_memory.TryGetValue(key, out var val)) return Task.FromResult((T?)val);
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        _memory[key] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _memory.Remove(key);
        return Task.CompletedTask;
    }
}

public class DummyNotificationService : IRealtimeNotificationService
{
    public Task NotifyVoteUpdatedAsync(string pollCode, PollResultsDto results) => Task.CompletedTask;
    public Task NotifyQuestionAddedAsync(string pollCode, QnADto question) => Task.CompletedTask;
}
