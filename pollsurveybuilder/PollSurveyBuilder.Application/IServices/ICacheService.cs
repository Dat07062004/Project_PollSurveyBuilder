using System;
using System.Threading.Tasks;

namespace PollSurveyBuilder.Application.IServices;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null);
    Task RemoveAsync(string key);
}
