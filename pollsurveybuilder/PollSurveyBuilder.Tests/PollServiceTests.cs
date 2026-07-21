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
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already voted*");
    }

    [Fact]
    public async Task SubmitVoteAsync_ShouldPreventDuplicateVotes_WhenVotingAnonymousThenLoggedIn()
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
            Title = "Test Poll 2",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "Option X", "Option Y" }
        });

        var voterToken = "browser_fingerprint_123";

        // Step 1: Vote anonymously
        await voteService.CastVoteAsync(poll.Code, new SubmitVoteRequest { OptionIndex = 0, VoterToken = voterToken }, userId: null);

        // Step 2: Try voting again after logging in from the same browser
        var userId = Guid.NewGuid();
        Func<Task> act = async () => await voteService.CastVoteAsync(poll.Code, new SubmitVoteRequest { OptionIndex = 1, VoterToken = voterToken }, userId: userId);

        // Assert: Must be blocked because the browser (VoterToken) has already voted
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already voted*");
    }

    [Fact]
    public async Task GetPollByCodeAsync_ShouldReturnPoll_WhenCodeExists()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var service = new PollService(pollRepo, new DummyCacheService());

        var created = await service.CreatePollAsync(new CreatePollRequest
        {
            Title = "Sample Poll",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "Yes", "No" }
        });

        // Act
        var result = await service.GetPollByCodeAsync(created.Code);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be(created.Code);
        result.Title.Should().Be("Sample Poll");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetPollByCodeAsync_ShouldReturnNull_WhenCodeDoesNotExist()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var service = new PollService(pollRepo, new DummyCacheService());

        // Act
        var result = await service.GetPollByCodeAsync("NONEXI");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ClosePollAsync_ShouldClosePoll_WhenOwner()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var service = new PollService(pollRepo, new DummyCacheService());
        var ownerId = Guid.NewGuid();

        var created = await service.CreatePollAsync(new CreatePollRequest
        {
            Title = "Closable Poll",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "A", "B" }
        }, ownerId);

        // Act
        var success = await service.ClosePollAsync(created.Code, ownerId);

        // Assert
        success.Should().BeTrue();
        var poll = await service.GetPollByCodeAsync(created.Code);
        poll!.IsClosed.Should().BeTrue();
        poll.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ClosePollAsync_ShouldThrowUnauthorized_WhenNotOwner()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var service = new PollService(pollRepo, new DummyCacheService());
        var ownerId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();

        var created = await service.CreatePollAsync(new CreatePollRequest
        {
            Title = "Protected Poll",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "A", "B" }
        }, ownerId);

        // Act & Assert
        Func<Task> act = async () => await service.ClosePollAsync(created.Code, strangerId);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeletePollAsync_ShouldDeletePoll_WhenOwner()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var service = new PollService(pollRepo, new DummyCacheService());
        var ownerId = Guid.NewGuid();

        var created = await service.CreatePollAsync(new CreatePollRequest
        {
            Title = "Deletable Poll",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "X" }
        }, ownerId);

        // Act
        var success = await service.DeletePollAsync(created.Code, ownerId);

        // Assert
        success.Should().BeTrue();
        var poll = await service.GetPollByCodeAsync(created.Code);
        poll.Should().BeNull();
    }

    [Fact]
    public async Task DeletePollAsync_ShouldThrowUnauthorized_WhenNotOwner()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var service = new PollService(pollRepo, new DummyCacheService());
        var ownerId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();

        var created = await service.CreatePollAsync(new CreatePollRequest
        {
            Title = "Protected Deletable Poll",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "X" }
        }, ownerId);

        // Act & Assert
        Func<Task> act = async () => await service.DeletePollAsync(created.Code, strangerId);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
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
