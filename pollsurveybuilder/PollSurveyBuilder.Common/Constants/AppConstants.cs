namespace PollSurveyBuilder.Common.Constants;

public static class CacheKeys
{
    public static string PollDetails(string code) => $"poll:details:{code}";
    public static string PollResults(string code) => $"poll:results:{code}";
    public static string VoterStatus(Guid pollId, string token) => $"voted:{pollId}:{token}";
}

public static class AppConstants
{
    public const int MaxOptions = 6;
    public const int DefaultExpiryHours = 24;
    public const string DefaultVoterCookieName = "PollSurvey_VoterToken";
}
