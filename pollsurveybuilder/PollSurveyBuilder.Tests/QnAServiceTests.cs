using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.Services;
using PollSurveyBuilder.Domain.Enums;
using PollSurveyBuilder.Infrastructure.DbContexts;
using PollSurveyBuilder.Infrastructure.Repositories;
using Xunit;

namespace PollSurveyBuilder.Tests;

public class QnAServiceTests
{
    private PollDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<PollDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PollDbContext(options);
    }

    [Fact]
    public async Task SubmitQuestionAsync_ShouldAddQuestionToPoll()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var qnaRepo = new QnARepository(db);
        var cacheService = new DummyCacheService();
        var notifService = new DummyNotificationService();
        var pollService = new PollService(pollRepo, cacheService);
        var qnaService = new QnAService(pollRepo, qnaRepo, notifService);

        var poll = await pollService.CreatePollAsync(new CreatePollRequest
        {
            Title = "QnA Poll",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "A" }
        });

        // Act
        var result = await qnaService.SubmitQuestionAsync(poll.Code, new CreateQnARequest
        {
            QuestionText = "How does real-time sync work?",
            VoterToken = "voter_q1"
        });

        // Assert
        result.Should().NotBeNull();
        result.QuestionText.Should().Be("How does real-time sync work?");
        result.Upvotes.Should().Be(0);
        result.IsPinned.Should().BeFalse();
    }

    [Fact]
    public async Task GetQuestionsAsync_ShouldSortByPinnedAndUpvoteCount()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var qnaRepo = new QnARepository(db);
        var cacheService = new DummyCacheService();
        var notifService = new DummyNotificationService();
        var pollService = new PollService(pollRepo, cacheService);
        var qnaService = new QnAService(pollRepo, qnaRepo, notifService);

        var poll = await pollService.CreatePollAsync(new CreatePollRequest
        {
            Title = "QnA Sorting Poll",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "A" }
        });

        var q1 = await qnaService.SubmitQuestionAsync(poll.Code, new CreateQnARequest { QuestionText = "Q1 (1 upvote)", VoterToken = "v1" });
        var q2 = await qnaService.SubmitQuestionAsync(poll.Code, new CreateQnARequest { QuestionText = "Q2 (5 upvotes)", VoterToken = "v2" });
        var q3 = await qnaService.SubmitQuestionAsync(poll.Code, new CreateQnARequest { QuestionText = "Q3 (Pinned)", VoterToken = "v3" });

        await qnaService.UpvoteQuestionAsync(q1.Id);
        for (int i = 0; i < 5; i++) await qnaService.UpvoteQuestionAsync(q2.Id);
        await qnaService.TogglePinAsync(q3.Id);

        // Act
        var questions = await qnaService.GetQuestionsAsync(poll.Code);

        // Assert
        questions.Should().HaveCount(3);
        questions[0].Id.Should().Be(q3.Id); // Pinned question comes first
        questions[1].Id.Should().Be(q2.Id); // Highest upvotes (5)
        questions[2].Id.Should().Be(q1.Id); // Lower upvotes (1)
    }

    [Fact]
    public async Task UpvoteQuestionAsync_ShouldIncrementUpvoteCount()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var pollRepo = new PollRepository(db);
        var qnaRepo = new QnARepository(db);
        var cacheService = new DummyCacheService();
        var notifService = new DummyNotificationService();
        var pollService = new PollService(pollRepo, cacheService);
        var qnaService = new QnAService(pollRepo, qnaRepo, notifService);

        var poll = await pollService.CreatePollAsync(new CreatePollRequest
        {
            Title = "Upvote Poll",
            QuestionType = QuestionType.MultipleChoice,
            Options = new List<string> { "A" }
        });

        var q = await qnaService.SubmitQuestionAsync(poll.Code, new CreateQnARequest { QuestionText = "Test question", VoterToken = "v1" });

        // Act
        var success = await qnaService.UpvoteQuestionAsync(q.Id);

        // Assert
        success.Should().BeTrue();
        var questions = await qnaService.GetQuestionsAsync(poll.Code);
        questions.First(x => x.Id == q.Id).Upvotes.Should().Be(1);
    }
}
