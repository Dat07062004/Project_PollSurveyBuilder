using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.Services;
using PollSurveyBuilder.Infrastructure.DbContexts;
using PollSurveyBuilder.Infrastructure.Repositories;
using Xunit;

namespace PollSurveyBuilder.Tests;

public class AuthServiceTests
{
    private PollDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<PollDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PollDbContext(options);
    }

    private IConfiguration GetMockConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:SecretKey", "SuperSecretPollSurveyBuilderKey2026_MustBeLongEnoughForHS256!" },
            { "Jwt:Issuer", "PollSurveyBuilder" },
            { "Jwt:Audience", "PollSurveyBuilderUsers" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenCredentialsAreValid()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var userRepo = new UserRepository(db);
        var pollRepo = new PollRepository(db);
        var config = GetMockConfiguration();
        var authService = new AuthService(userRepo, pollRepo, config);

        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "testuser@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await authService.RegisterAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Username.Should().Be("testuser");
        response.Email.Should().Be("testuser@example.com");
        response.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var userRepo = new UserRepository(db);
        var pollRepo = new PollRepository(db);
        var config = GetMockConfiguration();
        var authService = new AuthService(userRepo, pollRepo, config);

        await authService.RegisterAsync(new RegisterRequest
        {
            Username = "user1",
            Email = "duplicate@example.com",
            Password = "Password123!"
        });

        // Act & Assert
        Func<Task> act = async () => await authService.RegisterAsync(new RegisterRequest
        {
            Username = "user2",
            Email = "duplicate@example.com",
            Password = "Password123!"
        });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already in use*");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var userRepo = new UserRepository(db);
        var pollRepo = new PollRepository(db);
        var config = GetMockConfiguration();
        var authService = new AuthService(userRepo, pollRepo, config);

        await authService.RegisterAsync(new RegisterRequest
        {
            Username = "loginuser",
            Email = "loginuser@example.com",
            Password = "MySecurePassword123!"
        });

        // Act
        var response = await authService.LoginAsync(new LoginRequest
        {
            UsernameOrEmail = "loginuser@example.com",
            Password = "MySecurePassword123!"
        });

        // Assert
        response.Should().NotBeNull();
        response.Username.Should().Be("loginuser");
        response.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsIncorrect()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var userRepo = new UserRepository(db);
        var pollRepo = new PollRepository(db);
        var config = GetMockConfiguration();
        var authService = new AuthService(userRepo, pollRepo, config);

        await authService.RegisterAsync(new RegisterRequest
        {
            Username = "wrongpassuser",
            Email = "wrongpass@example.com",
            Password = "CorrectPassword123!"
        });

        // Act & Assert
        Func<Task> act = async () => await authService.LoginAsync(new LoginRequest
        {
            UsernameOrEmail = "wrongpassuser",
            Password = "WrongPassword!"
        });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Invalid*");
    }
}
