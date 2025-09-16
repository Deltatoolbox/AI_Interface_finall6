using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SimpleGateway.Data;
using SimpleGateway.Models;

namespace SimpleGateway.Services;

public interface IEncryptionService
{
    Task<string> EncryptMessageAsync(string content, string userId);
    Task<string> DecryptMessageAsync(string encryptedContent, string encryptionKeyId, string userId);
    Task<string> GenerateNewEncryptionKeyAsync(string userId);
    Task<bool> RotateEncryptionKeyAsync(string userId);
    Task<bool> IsEncryptionEnabledAsync(string userId);
    Task<bool> EnableEncryptionAsync(string userId);
    Task<bool> DisableEncryptionAsync(string userId);
    Task<EncryptionKey?> GetActiveEncryptionKeyAsync(string userId);
    Task<List<EncryptionKey>> GetUserEncryptionKeysAsync(string userId);
    Task<bool> DeactivateEncryptionKeyAsync(string keyId);
    Task<int> DeleteExpiredKeysAsync();
}

public class EncryptionService : IEncryptionService
{
    private readonly GatewayDbContext _context;
    private readonly ILogger<EncryptionService> _logger;
    private const int KeySize = 256; // AES-256
    private const int IvSize = 12; // GCM recommended IV size
    private const int TagSize = 16; // GCM tag size

    public EncryptionService(GatewayDbContext context, ILogger<EncryptionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> EncryptMessageAsync(string content, string userId)
    {
        try
        {
            // Check if encryption is enabled for user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || !user.EncryptionEnabled)
            {
                return content; // Return unencrypted if encryption is disabled
            }

            // Get active encryption key
            var activeKey = await GetActiveEncryptionKeyAsync(userId);
            if (activeKey == null)
            {
                _logger.LogWarning("No active encryption key found for user {UserId}", userId);
                return content; // Return unencrypted if no key
            }

            // Generate random IV for each message
            var iv = new byte[IvSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            // Encrypt the content
            var keyBytes = Convert.FromBase64String(activeKey.Key);
            var contentBytes = Encoding.UTF8.GetBytes(content);
            
            using var aes = new AesGcm(keyBytes);
            var encryptedBytes = new byte[contentBytes.Length];
            var tag = new byte[TagSize];
            
            aes.Encrypt(iv, contentBytes, encryptedBytes, tag);

            // Store encryption metadata
            var encryptedContent = Convert.ToBase64String(encryptedBytes);
            var ivBase64 = Convert.ToBase64String(iv);
            var tagBase64 = Convert.ToBase64String(tag);

            _logger.LogDebug("Message encrypted for user {UserId} with key {KeyId}", userId, activeKey.Id);

            // Return encrypted content with metadata (for storage)
            return $"{encryptedContent}|{ivBase64}|{tagBase64}|{activeKey.Id}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt message for user {UserId}", userId);
            throw new InvalidOperationException("Failed to encrypt message", ex);
        }
    }

    public async Task<string> DecryptMessageAsync(string encryptedData, string encryptionKeyId, string userId)
    {
        try
        {
            // Check if encryption is enabled for user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || !user.EncryptionEnabled)
            {
                return encryptedData; // Return as-is if encryption is disabled
            }

            // Parse encrypted data
            var parts = encryptedData.Split('|');
            if (parts.Length != 4)
            {
                _logger.LogWarning("Invalid encrypted data format for user {UserId}", userId);
                return encryptedData; // Return as-is if format is invalid
            }

            var encryptedContent = parts[0];
            var ivBase64 = parts[1];
            var tagBase64 = parts[2];
            var keyId = parts[3];

            // Get encryption key
            var encryptionKey = await _context.EncryptionKeys
                .FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId);

            if (encryptionKey == null)
            {
                _logger.LogWarning("Encryption key {KeyId} not found for user {UserId}", keyId, userId);
                return encryptedData; // Return as-is if key not found
            }

            // Decrypt the content
            var keyBytes = Convert.FromBase64String(encryptionKey.Key);
            var iv = Convert.FromBase64String(ivBase64);
            var tag = Convert.FromBase64String(tagBase64);
            var encryptedBytes = Convert.FromBase64String(encryptedContent);

            using var aes = new AesGcm(keyBytes);
            var decryptedBytes = new byte[encryptedBytes.Length];
            
            aes.Decrypt(iv, encryptedBytes, tag, decryptedBytes);

            var decryptedContent = Encoding.UTF8.GetString(decryptedBytes);

            _logger.LogDebug("Message decrypted for user {UserId} with key {KeyId}", userId, keyId);

            return decryptedContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt message for user {UserId}", userId);
            throw new InvalidOperationException("Failed to decrypt message", ex);
        }
    }

    public async Task<string> GenerateNewEncryptionKeyAsync(string userId)
    {
        try
        {
            // Generate new AES-256 key
            var keyBytes = new byte[KeySize / 8]; // 32 bytes for AES-256
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }

            var keyBase64 = Convert.ToBase64String(keyBytes);
            var keyId = Guid.NewGuid().ToString();

            // Create new encryption key
            var encryptionKey = new EncryptionKey
            {
                Id = keyId,
                UserId = userId,
                Key = keyBase64,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Version = 1
            };

            // Deactivate old keys for this user
            var oldKeys = await _context.EncryptionKeys
                .Where(k => k.UserId == userId && k.IsActive)
                .ToListAsync();

            foreach (var oldKey in oldKeys)
            {
                oldKey.IsActive = false;
                oldKey.DeactivatedAt = DateTime.UtcNow;
            }

            // Add new key
            _context.EncryptionKeys.Add(encryptionKey);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New encryption key generated for user {UserId}", userId);

            return keyId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate new encryption key for user {UserId}", userId);
            throw new InvalidOperationException("Failed to generate encryption key", ex);
        }
    }

    public async Task<bool> RotateEncryptionKeyAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || !user.EncryptionEnabled)
            {
                return false;
            }

            // Generate new key
            var newKeyId = await GenerateNewEncryptionKeyAsync(userId);

            // Update user's key rotation timestamp
            user.LastKeyRotation = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Encryption key rotated for user {UserId}", userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rotate encryption key for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsEncryptionEnabledAsync(string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user?.EncryptionEnabled ?? false;
    }

    public async Task<bool> EnableEncryptionAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            if (!user.EncryptionEnabled)
            {
                // Generate initial encryption key
                await GenerateNewEncryptionKeyAsync(userId);
                
                user.EncryptionEnabled = true;
                user.EncryptionEnabledAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Encryption enabled for user {UserId}", userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable encryption for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DisableEncryptionAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            if (user.EncryptionEnabled)
            {
                // Deactivate all encryption keys
                var activeKeys = await _context.EncryptionKeys
                    .Where(k => k.UserId == userId && k.IsActive)
                    .ToListAsync();

                foreach (var key in activeKeys)
                {
                    key.IsActive = false;
                    key.DeactivatedAt = DateTime.UtcNow;
                }

                user.EncryptionEnabled = false;
                user.EncryptionDisabledAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Encryption disabled for user {UserId}", userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable encryption for user {UserId}", userId);
            return false;
        }
    }

    public async Task<EncryptionKey?> GetActiveEncryptionKeyAsync(string userId)
    {
        return await _context.EncryptionKeys
            .FirstOrDefaultAsync(k => k.UserId == userId && k.IsActive);
    }

    public async Task<List<EncryptionKey>> GetUserEncryptionKeysAsync(string userId)
    {
        return await _context.EncryptionKeys
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> DeactivateEncryptionKeyAsync(string keyId)
    {
        try
        {
            var key = await _context.EncryptionKeys
                .FirstOrDefaultAsync(k => k.Id == keyId);

            if (key == null)
                return false;

            key.IsActive = false;
            key.DeactivatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Encryption key {KeyId} deactivated", keyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate encryption key {KeyId}", keyId);
            return false;
        }
    }

    public async Task<int> DeleteExpiredKeysAsync()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-365); // Delete keys older than 1 year
            var expiredKeys = await _context.EncryptionKeys
                .Where(k => k.CreatedAt < cutoffDate && !k.IsActive)
                .ToListAsync();

            if (expiredKeys.Any())
            {
                _context.EncryptionKeys.RemoveRange(expiredKeys);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Deleted {Count} expired encryption keys", expiredKeys.Count);
            return expiredKeys.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete expired encryption keys");
            return 0;
        }
    }
}
