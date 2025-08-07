using System.Security.Cryptography;
using System.Text;
using Vault.Core.Interfaces;

namespace Vault.Core.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService()
    {
        // In production, this should come from configuration/environment variables
        // For demo purposes, using a fixed key
        var keyString = "MyVerySecureEncryptionKey123456!"; // 32 characters for AES-256
        _key = Encoding.UTF8.GetBytes(keyString);
    }

    public (string encryptedData, string initializationVector) Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        var iv = aes.IV;
        var encryptor = aes.CreateEncryptor(aes.Key, iv);

        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using var swEncrypt = new StreamWriter(csEncrypt);
        
        swEncrypt.Write(plainText);
        swEncrypt.Close();

        var encryptedBytes = msEncrypt.ToArray();
        
        return (Convert.ToBase64String(encryptedBytes), Convert.ToBase64String(iv));
    }

    public string Decrypt(string encryptedData, string initializationVector)
    {
        if (string.IsNullOrEmpty(encryptedData))
            throw new ArgumentException("Encrypted data cannot be null or empty", nameof(encryptedData));
        
        if (string.IsNullOrEmpty(initializationVector))
            throw new ArgumentException("Initialization vector cannot be null or empty", nameof(initializationVector));

        var encryptedBytes = Convert.FromBase64String(encryptedData);
        var iv = Convert.FromBase64String(initializationVector);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var msDecrypt = new MemoryStream(encryptedBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        
        return srDecrypt.ReadToEnd();
    }
}
