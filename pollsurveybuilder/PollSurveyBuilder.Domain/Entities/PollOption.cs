using System;

namespace PollSurveyBuilder.Domain.Entities;

public class PollOption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PollId { get; set; }
    public int OptionIndex { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int VoteCount { get; set; } = 0;

    public Poll? Poll { get; set; }
}
