namespace Vault.Core.Interfaces;

public interface IEncryptionService
{
    (string encryptedData, string initializationVector) Encrypt(string plainText);
    string Decrypt(string encryptedData, string initializationVector);
}

public interface IPasswordHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface IJwtTokenService
{
    string GenerateToken(int userId, string username);
    bool ValidateToken(string token);
    int GetUserIdFromToken(string token);
}
