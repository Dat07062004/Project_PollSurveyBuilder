using System;
using System.Threading.Tasks;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Application.IRepositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<bool> ExistsEmailAsync(string email);
    Task<bool> ExistsUsernameAsync(string username);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task SaveChangesAsync();
}
