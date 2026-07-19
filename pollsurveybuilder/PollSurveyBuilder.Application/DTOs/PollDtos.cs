using System;
using System.Collections.Generic;
using PollSurveyBuilder.Domain.Enums;

namespace PollSurveyBuilder.Application.DTOs;

public class CreatePollRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;
    public List<string> Options { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
    public int? ExpiresInHours { get; set; }
}

public class PollDetailsDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public QuestionType QuestionType { get; set; }
    public bool IsClosed { get; set; }
    public bool IsExpired { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PollOptionDto> Options { get; set; } = new();
    public bool HasVoted { get; set; }
}

public class PollOptionDto
{
    public Guid Id { get; set; }
    public int OptionIndex { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public double Percentage { get; set; }
}

public class SubmitVoteRequest
{
    public int OptionIndex { get; set; }
    public string? VoterToken { get; set; }
    public string? TextResponse { get; set; }
}

public class PollResultsDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public bool IsClosed { get; set; }
    public bool IsExpired { get; set; }
    public int TotalVotes { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<PollOptionDto> Options { get; set; } = new();
    public List<string> OpenTextResponses { get; set; } = new();
}

public class PollAnalyticsDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public string PeakVotingMinute { get; set; } = "N/A";
    public string TopOption { get; set; } = "N/A";
    public List<TimeSeriesVotePoint> VotesOverTime { get; set; } = new();
    public List<PollOptionDto> OptionDistribution { get; set; } = new();
}

public class TimeSeriesVotePoint
{
    public string TimeLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class QnADto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Upvotes { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateQnARequest
{
    public string QuestionText { get; set; } = string.Empty;
    public string? VoterToken { get; set; }
}
