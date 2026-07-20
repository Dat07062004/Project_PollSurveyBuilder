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
using PollSurveyBuilder.Domain.Enums;

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

    public async Task<PollDetailsDto> UpdatePollAsync(string code, UpdatePollRequest dto, Guid? userId = null)
    {
        var poll = await _pollRepository.GetByCodeAsync(code);
        if (poll == null) throw new KeyNotFoundException("Poll not found.");

        if (userId.HasValue && poll.CreatedByUserId.HasValue && poll.CreatedByUserId.Value != userId.Value)
        {
            throw new UnauthorizedAccessException("You do not have permission to edit this poll.");
        }

        string newTitle = !string.IsNullOrWhiteSpace(dto.Title) ? dto.Title.Trim() : poll.Title;
        string? newDesc = dto.Description != null ? (string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()) : poll.Description;
        DateTime? newExpiresAt = dto.ExpiresAt.HasValue ? dto.ExpiresAt.Value : poll.ExpiresAt;
        QuestionType newQuestionType = dto.QuestionType.HasValue ? dto.QuestionType.Value : poll.QuestionType;

        await _pollRepository.ExecuteUpdatePollRawAsync(poll.Id, newTitle, newDesc, (int)newQuestionType, newExpiresAt);

        if (dto.Options != null)
        {
            var validOptions = dto.Options.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToList();
            if (validOptions.Count > 0)
            {
                await _pollRepository.ExecuteReplaceOptionsRawAsync(poll.Id, validOptions);
            }
        }

        await _cacheService.RemoveAsync(CacheKeys.PollDetails(code));
        await _cacheService.RemoveAsync(CacheKeys.PollResults(code));

        var updatedPoll = await _pollRepository.GetByCodeAsync(code);
        return MapToDetailsDto(updatedPoll ?? poll);
    }

    public async Task<bool> DeletePollAsync(string code, Guid? userId = null)
    {
        var poll = await _pollRepository.GetByCodeAsync(code);
        if (poll == null) return false;

        if (userId.HasValue && poll.CreatedByUserId.HasValue && poll.CreatedByUserId.Value != userId.Value)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this poll.");
        }

        await _pollRepository.DeleteAsync(poll);
        await _pollRepository.SaveChangesAsync();

        await _cacheService.RemoveAsync(CacheKeys.PollDetails(code));
        await _cacheService.RemoveAsync(CacheKeys.PollResults(code));

        return true;
    }

    public async Task<bool> ClosePollAsync(string code, Guid? userId = null)
    {
        var poll = await _pollRepository.GetByCodeAsync(code);
        if (poll == null) return false;

        if (userId.HasValue && poll.CreatedByUserId.HasValue && poll.CreatedByUserId.Value != userId.Value)
        {
            throw new UnauthorizedAccessException("You do not have permission to close this poll.");
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
            ExpiresAt = poll.ExpiresAt.HasValue ? DateTime.SpecifyKind(poll.ExpiresAt.Value, DateTimeKind.Utc) : null,
            CreatedAt = DateTime.SpecifyKind(poll.CreatedAt, DateTimeKind.Utc),
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
