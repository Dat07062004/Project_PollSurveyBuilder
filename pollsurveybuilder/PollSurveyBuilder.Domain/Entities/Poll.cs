using System;
using System.Collections.Generic;
using PollSurveyBuilder.Domain.Enums;

namespace PollSurveyBuilder.Domain.Entities;

public class Poll
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;
    public bool IsClosed { get; set; } = false;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public ICollection<QnAQuestion> QnAQuestions { get; set; } = new List<QnAQuestion>();

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt.Value;
    public bool IsActive => !IsClosed && !IsExpired;
}
