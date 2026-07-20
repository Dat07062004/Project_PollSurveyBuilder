using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PollSurveyBuilder.API.Middleware;
using PollSurveyBuilder.Application;
using PollSurveyBuilder.Infrastructure;
using PollSurveyBuilder.Infrastructure.DbContexts;
using PollSurveyBuilder.Infrastructure.Hubs;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Clean Architecture layer dependency injections
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Add JWT Bearer Authentication
var jwtKey = builder.Configuration["Jwt:SecretKey"] ?? "SuperSecretPollSurveyBuilderKey2026_MustBeLongEnoughForHS256!";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PollSurveyBuilder",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PollSurveyBuilderUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// 3. Add SignalR Realtime WebSocket service
builder.Services.AddSignalR();

// 4. Add Controllers & OpenAPI Scalar documentation
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 5. Configure CORS for SPA frontend & WebSockets
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 6. Auto Ensure DB Created & Auto Migration for Users table on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<PollDbContext>();
        await db.Database.EnsureCreatedAsync();

        // Create Users table if not exists in MySQL
        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS `Users` (
              `Id` char(36) NOT NULL,
              `Email` varchar(150) NOT NULL,
              `Username` varchar(100) NOT NULL,
              `PasswordHash` longtext NULL,
              `AuthProvider` varchar(50) NOT NULL DEFAULT 'Local',
              `CreatedAt` datetime NOT NULL,
              PRIMARY KEY (`Id`),
              UNIQUE KEY `IX_Users_Email` (`Email`),
              UNIQUE KEY `IX_Users_Username` (`Username`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");

        // Note: CreatedByUserId and UserId columns are already defined in PollDbContext and created by EnsureCreatedAsync()
        logger.LogInformation("Database verified & Users table created successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to initialize SQL Server / MySQL database automatically. Ensure connection string is valid.");
    }
}

// 7. Global Exception Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 8. OpenAPI Scalar UI for .NET 10
if (app.Environment.IsDevelopment() || true)
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Poll & Survey Builder API - Scalar")
               .WithTheme(ScalarTheme.Purple);
    });
}

app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PollHub>("/hubs/poll");

app.Run();
