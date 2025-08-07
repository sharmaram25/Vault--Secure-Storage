using System.Security.Cryptography;
using System.Text;
using Vault.Core.Interfaces;

namespace Vault.Core.Services;

public class PasswordHashingService : IPasswordHashingService
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        using var algorithm = new Rfc2898DeriveBytes(
            password,
            SaltSize,
            Iterations,
            HashAlgorithmName.SHA256);

        var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
        var salt = Convert.ToBase64String(algorithm.Salt);

        return $"{Iterations}.{salt}.{key}";
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        
        if (string.IsNullOrEmpty(hash))
            throw new ArgumentException("Hash cannot be null or empty", nameof(hash));

        var parts = hash.Split('.', 3);
        if (parts.Length != 3)
            return false;

        var iterations = Convert.ToInt32(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var key = Convert.FromBase64String(parts[2]);

        using var algorithm = new Rfc2898DeriveBytes(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256);

        var keyToCheck = algorithm.GetBytes(KeySize);

        return keyToCheck.SequenceEqual(key);
    }
}
