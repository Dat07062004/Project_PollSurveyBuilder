using System;

namespace PollSurveyBuilder.Domain.Entities;

public class QnAQuestion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PollId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string VoterToken { get; set; } = string.Empty;
    public int Upvotes { get; set; } = 0;
    public bool IsPinned { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Poll? Poll { get; set; }
}
