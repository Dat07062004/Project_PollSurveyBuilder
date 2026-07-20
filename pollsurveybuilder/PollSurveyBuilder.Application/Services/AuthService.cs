using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IRepositories;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPollRepository _pollRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IPollRepository pollRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _pollRepository = pollRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.ExistsEmailAsync(request.Email))
        {
            throw new InvalidOperationException("This email address is already in use.");
        }

        if (await _userRepository.ExistsUsernameAsync(request.Username))
        {
            throw new InvalidOperationException("This username is already taken.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email.Trim().ToLower(),
            Username = request.Username.Trim(),
            PasswordHash = passwordHash,
            AuthProvider = "Local",
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail.Trim());
        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid username/email or password.");
        }

        bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!valid)
        {
            throw new InvalidOperationException("Invalid username/email or password.");
        }

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleAuthRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
        {
            // Auto register new Google user
            var baseUsername = !string.IsNullOrEmpty(request.Name) ? request.Name.Replace(" ", "_").ToLower() : email.Split('@')[0];
            var username = baseUsername;
            int counter = 1;
            while (await _userRepository.ExistsUsernameAsync(username))
            {
                username = $"{baseUsername}_{counter++}";
            }

            user = new User
            {
                Email = email,
                Username = username,
                PasswordHash = null,
                AuthProvider = "Google",
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }

    public async Task<List<UserPollDto>> GetUserPollsAsync(Guid userId)
    {
        var polls = await _pollRepository.GetPollsByUserIdAsync(userId);
        return polls.Select(p => new UserPollDto
        {
            Id = p.Id,
            Code = p.Code,
            Title = p.Title,
            Description = p.Description,
            QuestionType = p.QuestionType.ToString(),
            IsClosed = p.IsClosed,
            IsExpired = p.IsExpired,
            ExpiresAt = p.ExpiresAt,
            CreatedAt = p.CreatedAt,
            TotalVotes = p.Votes.Count
        }).ToList();
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:SecretKey"] ?? "SuperSecretPollSurveyBuilderKey2026_MustBeLongEnoughForHS256!";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("AuthProvider", user.AuthProvider)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "PollSurveyBuilder",
            audience: _configuration["Jwt:Audience"] ?? "PollSurveyBuilderUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
