using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IRepositories;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Common.Constants;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Application.Services;

public class VoteService : IVoteService
{
    private readonly IPollRepository _pollRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly ICacheService _cacheService;
    private readonly IRealtimeNotificationService _notificationService;

    public VoteService(
        IPollRepository pollRepository,
        IVoteRepository voteRepository,
        ICacheService cacheService,
        IRealtimeNotificationService notificationService)
    {
        _pollRepository = pollRepository;
        _voteRepository = voteRepository;
        _cacheService = cacheService;
        _notificationService = notificationService;
    }

    public async Task<PollResultsDto> CastVoteAsync(string code, SubmitVoteRequest dto, Guid? userId = null)
    {
        var poll = await _pollRepository.GetByCodeAsync(code);
        if (poll == null) throw new KeyNotFoundException("Poll not found.");
        if (poll.IsClosed) throw new InvalidOperationException("This poll is closed.");
        if (poll.IsExpired) throw new InvalidOperationException("This poll has expired.");

        if (!string.IsNullOrEmpty(dto.VoterToken))
        {
            bool hasVotedToken = await _voteRepository.HasVotedAsync(poll.Id, dto.VoterToken);
            if (hasVotedToken) throw new InvalidOperationException("You (or this browser) have already voted in this poll.");
        }

        if (userId.HasValue)
        {
            bool hasVotedUser = await _voteRepository.HasUserVotedAsync(poll.Id, userId.Value);
            if (hasVotedUser) throw new InvalidOperationException("Your account has already voted in this poll.");
        }

        var vote = new Vote
        {
            PollId = poll.Id,
            OptionIndex = dto.OptionIndex,
            VoterToken = dto.VoterToken ?? Guid.NewGuid().ToString(),
            TextResponse = dto.TextResponse,
            UserId = userId,
            VotedAt = DateTime.UtcNow
        };

        await _voteRepository.AddAsync(vote);
        await _voteRepository.SaveChangesAsync();

        await _cacheService.RemoveAsync(CacheKeys.PollResults(code));

        var updatedResults = await GetPollResultsAsync(code);
        await _notificationService.NotifyVoteUpdatedAsync(code, updatedResults);

        return updatedResults;
    }

    public async Task<PollResultsDto> GetPollResultsAsync(string code)
    {
        var cacheKey = CacheKeys.PollResults(code);
        var cached = await _cacheService.GetAsync<PollResultsDto>(cacheKey);
        if (cached != null)
        {
            bool isExpired = cached.ExpiresAt.HasValue && DateTime.UtcNow >= cached.ExpiresAt.Value;
            cached.IsExpired = isExpired;
            return cached;
        }

        var poll = await _pollRepository.GetByCodeAsync(code);
        if (poll == null) throw new KeyNotFoundException("Poll not found.");

        var votes = await _voteRepository.GetVotesByPollIdAsync(poll.Id);
        int totalVotes = votes.Count;

        var optionCounts = new Dictionary<int, int>();
        var textResponses = new List<string>();

        foreach (var vote in votes)
        {
            if (!optionCounts.ContainsKey(vote.OptionIndex))
                optionCounts[vote.OptionIndex] = 0;
            optionCounts[vote.OptionIndex]++;

            if (!string.IsNullOrWhiteSpace(vote.TextResponse))
            {
                textResponses.Add(vote.TextResponse);
            }
        }

        var optionResults = poll.Options.OrderBy(o => o.OptionIndex).Select(opt =>
        {
            int count = optionCounts.ContainsKey(opt.OptionIndex) ? optionCounts[opt.OptionIndex] : 0;
            double pct = totalVotes > 0 ? Math.Round((double)count / totalVotes * 100, 1) : 0;
            return new PollOptionDto
            {
                Id = opt.Id,
                OptionIndex = opt.OptionIndex,
                OptionText = opt.OptionText,
                VoteCount = count,
                Percentage = pct
            };
        }).ToList();

        bool pollIsExpired = poll.ExpiresAt.HasValue && DateTime.UtcNow >= poll.ExpiresAt.Value;

        var dto = new PollResultsDto
        {
            Code = poll.Code,
            Title = poll.Title,
            QuestionType = poll.QuestionType,
            IsClosed = poll.IsClosed,
            IsExpired = pollIsExpired,
            TotalVotes = totalVotes,
            ExpiresAt = poll.ExpiresAt.HasValue ? DateTime.SpecifyKind(poll.ExpiresAt.Value, DateTimeKind.Utc) : null,
            Options = optionResults,
            OpenTextResponses = textResponses
        };

        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromSeconds(10));
        return dto;
    }

    public async Task<PollAnalyticsDto> GetPollAnalyticsAsync(string code)
    {
        var poll = await _pollRepository.GetByCodeAsync(code);
        if (poll == null) throw new KeyNotFoundException("Poll not found.");

        var votes = await _voteRepository.GetVotesByPollIdAsync(poll.Id);

        var timeSeries = votes
            .GroupBy(v => DateTime.SpecifyKind(v.VotedAt, DateTimeKind.Utc).ToLocalTime().ToString("yyyy-MM-dd HH:mm"))
            .Select(g => new TimeSeriesVotePoint
            {
                TimeLabel = g.Key,
                Count = g.Count()
            })
            .OrderBy(t => t.TimeLabel)
            .ToList();

        string peakMinute = timeSeries.OrderByDescending(v => v.Count).FirstOrDefault()?.TimeLabel ?? "N/A";

        var results = await GetPollResultsAsync(code);
        var topOption = results.Options.OrderByDescending(o => o.VoteCount).FirstOrDefault()?.OptionText ?? "N/A";

        return new PollAnalyticsDto
        {
            Code = poll.Code,
            Title = poll.Title,
            TotalVotes = votes.Count,
            PeakVotingMinute = peakMinute,
            TopOption = topOption,
            VotesOverTime = timeSeries,
            OptionDistribution = results.Options
        };
    }
}
