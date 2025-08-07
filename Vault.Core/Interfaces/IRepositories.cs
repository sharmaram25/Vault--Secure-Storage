using Vault.Core.Models;

namespace Vault.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> ExistsAsync(string username, string email);
}

public interface ISecretRepository
{
    Task<IEnumerable<Secret>> GetByUserIdAsync(int userId);
    Task<Secret?> GetByIdAsync(int id, int userId);
    Task<Secret> CreateAsync(Secret secret);
    Task<Secret> UpdateAsync(Secret secret);
    Task<bool> DeleteAsync(int id, int userId);
}
