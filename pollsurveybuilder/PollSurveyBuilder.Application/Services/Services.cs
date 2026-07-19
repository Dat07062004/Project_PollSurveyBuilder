using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IRepositories;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Common.Constants;
using PollSurveyBuilder.Common.Utils;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Application.Services;

public class PollService : IPollService
{
    private readonly IPollRepository _pollRepository;
    private readonly ICacheService _cacheService;

    public PollService(IPollRepository pollRepository, ICacheService cacheService)
    {
        _pollRepository = pollRepository;
        _cacheService = cacheService;
    }

    public async Task<PollDetailsDto> CreatePollAsync(CreatePollRequest dto, Guid? userId = null)
    {
        string code = ShortCodeGenerator.Generate();
        while (await _pollRepository.ExistsCodeAsync(code))
        {
            code = ShortCodeGenerator.Generate();
        }

        DateTime? expiresAt = dto.ExpiresAt;
        if (!expiresAt.HasValue && dto.ExpiresInHours.HasValue && dto.ExpiresInHours.Value > 0)
        {
            expiresAt = DateTime.UtcNow.AddHours(dto.ExpiresInHours.Value);
        }

        var poll = new Poll
        {
            Code = code,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            QuestionType = dto.QuestionType,
            ExpiresAt = expiresAt,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Options != null && dto.Options.Any())
        {
            int order = 0;
            foreach (var opt in dto.Options)
            {
                if (!string.IsNullOrWhiteSpace(opt))
                {
                    poll.Options.Add(new PollOption
                    {
                        OptionText = opt.Trim(),
                        OptionIndex = order++
                    });
                }
            }
        }

        await _pollRepository.AddAsync(poll);
        await _pollRepository.SaveChangesAsync();

        var result = MapToDetailsDto(poll);
        await _cacheService.SetAsync(CacheKeys.PollDetails(code), result, TimeSpan.FromMinutes(30));

        return result;
    }

    public async Task<PollDetailsDto?> GetPollByCodeAsync(string code)
    {
        var cacheKey = CacheKeys.PollDetails(code);
        var cached = await _cacheService.GetAsync<PollDetailsDto>(cacheKey);
        if (cached != null)
        {
            // Dynamically re-evaluate IsExpired & IsActive based on current UTC time
            bool isExpired = cached.ExpiresAt.HasValue && DateTime.UtcNow >= cached.ExpiresAt.Value;
            cached.IsExpired = isExpired;
            cached.IsActive = !cached.IsClosed && !isExpired;
            return cached;
        }

        var poll = await _pollRepository.GetByCodeAsync(code);
        if (poll == null) return null;

        var dto = MapToDetailsDto(poll);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));
        return dto;
    }

    public async Task<bool> ClosePollAsync(string code, Guid? userId = null)
    {
        var poll = await _pollRepository.GetByCodeAsync(code);
        if (poll == null) return false;

        if (userId.HasValue && poll.CreatedByUserId.HasValue && poll.CreatedByUserId.Value != userId.Value)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền đóng cuộc thăm dò này.");
        }

        poll.IsClosed = true;
        await _pollRepository.UpdateAsync(poll);
        await _pollRepository.SaveChangesAsync();

        await _cacheService.RemoveAsync(CacheKeys.PollDetails(code));
        await _cacheService.RemoveAsync(CacheKeys.PollResults(code));

        return true;
    }

    private static PollDetailsDto MapToDetailsDto(Poll poll)
    {
        bool isExpired = poll.ExpiresAt.HasValue && DateTime.UtcNow >= poll.ExpiresAt.Value;
        return new PollDetailsDto
        {
            Id = poll.Id,
            Code = poll.Code,
            Title = poll.Title,
            Description = poll.Description,
            QuestionType = poll.QuestionType,
            IsClosed = poll.IsClosed,
            IsExpired = isExpired,
            IsActive = !poll.IsClosed && !isExpired,
            ExpiresAt = poll.ExpiresAt,
            CreatedAt = poll.CreatedAt,
            Options = poll.Options.OrderBy(o => o.OptionIndex).Select(o => new PollOptionDto
            {
                Id = o.Id,
                OptionIndex = o.OptionIndex,
                OptionText = o.OptionText,
                VoteCount = o.VoteCount,
                Percentage = 0
            }).ToList()
        };
    }
}

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
        if (poll == null) throw new KeyNotFoundException("Không tìm thấy cuộc thăm dò.");
        if (poll.IsClosed) throw new InvalidOperationException("Cuộc thăm dò này đã đóng.");
        if (poll.IsExpired) throw new InvalidOperationException("Cuộc thăm dò này đã hết hạn.");

        // Single vote validation (Check BOTH VoterToken and UserId)
        if (!string.IsNullOrEmpty(dto.VoterToken))
        {
            bool hasVotedToken = await _voteRepository.HasVotedAsync(poll.Id, dto.VoterToken);
            if (hasVotedToken) throw new InvalidOperationException("Bạn (hoặc trình duyệt này) đã thực hiện bỏ phiếu cho cuộc thăm dò này rồi.");
        }

        if (userId.HasValue)
        {
            bool hasVotedUser = await _voteRepository.HasUserVotedAsync(poll.Id, userId.Value);
            if (hasVotedUser) throw new InvalidOperationException("Tài khoản của bạn đã thực hiện bình chọn cho cuộc thăm dò này rồi.");
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
        if (poll == null) throw new KeyNotFoundException("Không tìm thấy cuộc thăm dò.");

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
            ExpiresAt = poll.ExpiresAt,
            Options = optionResults,
            OpenTextResponses = textResponses
        };

        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromSeconds(10));
        return dto;
    }

    public async Task<PollAnalyticsDto> GetPollAnalyticsAsync(string code)
    {
        var poll = await _pollRepository.GetByCodeAsync(code);
        if (poll == null) throw new KeyNotFoundException("Không tìm thấy cuộc thăm dò.");

        var votes = await _voteRepository.GetVotesByPollIdAsync(poll.Id);

        var timeSeries = votes
            .GroupBy(v => v.VotedAt.ToString("yyyy-MM-dd HH:mm"))
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

public class QnAService : IQnAService
{
    private readonly IPollRepository _pollRepository;
    private readonly IQnARepository _qnaRepository;
    private readonly IRealtimeNotificationService _notificationService;

    public QnAService(
        IPollRepository pollRepository,
        IQnARepository qnaRepository,
        IRealtimeNotificationService notificationService)
    {
        _pollRepository = pollRepository;
        _qnaRepository = qnaRepository;
        _notificationService = notificationService;
    }

    public async Task<QnADto> SubmitQuestionAsync(string code, CreateQnARequest dto, Guid? userId = null)
    {
        var poll = await _pollRepository.GetByCodeAsync(code);
        if (poll == null) throw new KeyNotFoundException("Không tìm thấy cuộc thăm dò.");

        var question = new QnAQuestion
        {
            PollId = poll.Id,
            QuestionText = dto.QuestionText.Trim(),
            VoterToken = dto.VoterToken ?? Guid.NewGuid().ToString(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _qnaRepository.AddAsync(question);
        await _qnaRepository.SaveChangesAsync();

        var qnaDto = MapToDto(question);
        await _notificationService.NotifyQuestionAddedAsync(code, qnaDto);

        return qnaDto;
    }

    public async Task<List<QnADto>> GetQuestionsAsync(string pollCode)
    {
        var poll = await _pollRepository.GetByCodeAsync(pollCode);
        if (poll == null) throw new KeyNotFoundException("Không tìm thấy cuộc thăm dò.");

        var questions = await _qnaRepository.GetQuestionsByPollIdAsync(poll.Id);
        return questions.Select(MapToDto).ToList();
    }

    public async Task<bool> UpvoteQuestionAsync(Guid questionId)
    {
        var question = await _qnaRepository.GetByIdAsync(questionId);
        if (question == null) return false;

        question.Upvotes++;
        await _qnaRepository.UpdateAsync(question);
        await _qnaRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> TogglePinAsync(Guid questionId)
    {
        var question = await _qnaRepository.GetByIdAsync(questionId);
        if (question == null) return false;

        question.IsPinned = !question.IsPinned;
        await _qnaRepository.UpdateAsync(question);
        await _qnaRepository.SaveChangesAsync();

        return true;
    }

    private static QnADto MapToDto(QnAQuestion q) => new QnADto
    {
        Id = q.Id,
        QuestionText = q.QuestionText,
        Upvotes = q.Upvotes,
        IsPinned = q.IsPinned,
        CreatedAt = q.CreatedAt
    };
}
