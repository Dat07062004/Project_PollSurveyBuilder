using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PollSurveyBuilder.Application.DTOs;

namespace PollSurveyBuilder.Application.IServices;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequest request);
    Task<AuthResponseDto> LoginAsync(LoginRequest request);
    Task<AuthResponseDto> GoogleLoginAsync(GoogleAuthRequest request);
    Task<List<UserPollDto>> GetUserPollsAsync(Guid userId);
}
