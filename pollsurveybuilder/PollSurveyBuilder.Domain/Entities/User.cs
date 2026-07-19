using System;
using System.Collections.Generic;

namespace PollSurveyBuilder.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string AuthProvider { get; set; } = "Local"; // "Local" or "Google"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Poll> Polls { get; set; } = new List<Poll>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public ICollection<QnAQuestion> QnAQuestions { get; set; } = new List<QnAQuestion>();
}
