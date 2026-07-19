using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PollSurveyBuilder.Application.IRepositories;
using PollSurveyBuilder.Domain.Entities;
using PollSurveyBuilder.Infrastructure.DbContexts;

namespace PollSurveyBuilder.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PollDbContext _context;

    public UserRepository(PollDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        var lower = usernameOrEmail.ToLower();
        return await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == lower || u.Email.ToLower() == lower);
    }

    public async Task<bool> ExistsEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> ExistsUsernameAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
