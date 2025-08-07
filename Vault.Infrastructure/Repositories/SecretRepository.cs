using Microsoft.EntityFrameworkCore;
using Vault.Core.Interfaces;
using Vault.Core.Models;
using Vault.Infrastructure.Data;

namespace Vault.Infrastructure.Repositories;

public class SecretRepository : ISecretRepository
{
    private readonly VaultDbContext _context;

    public SecretRepository(VaultDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Secret>> GetByUserIdAsync(int userId)
    {
        return await _context.Secrets
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Secret?> GetByIdAsync(int id, int userId)
    {
        return await _context.Secrets
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    public async Task<Secret> CreateAsync(Secret secret)
    {
        secret.CreatedAt = DateTime.UtcNow;
        _context.Secrets.Add(secret);
        await _context.SaveChangesAsync();
        return secret;
    }

    public async Task<Secret> UpdateAsync(Secret secret)
    {
        secret.UpdatedAt = DateTime.UtcNow;
        _context.Secrets.Update(secret);
        await _context.SaveChangesAsync();
        return secret;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var secret = await _context.Secrets
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        
        if (secret == null)
            return false;

        _context.Secrets.Remove(secret);
        await _context.SaveChangesAsync();
        return true;
    }
}
