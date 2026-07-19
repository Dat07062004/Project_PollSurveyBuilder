using System;

namespace PollSurveyBuilder.Domain.Entities;

public class Vote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PollId { get; set; }
    public int OptionIndex { get; set; }
    public string VoterToken { get; set; } = string.Empty;
    public string? TextResponse { get; set; }
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Poll? Poll { get; set; }
}
