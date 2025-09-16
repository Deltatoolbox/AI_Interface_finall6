using System.ComponentModel.DataAnnotations;

namespace SimpleGateway.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Role { get; set; } = "User";
    
    public string? RoleId { get; set; }
    
    public bool IsGuest { get; set; } = false;
    public string? SessionId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    public bool IsSsoUser { get; set; } = false;
    public string? SsoProvider { get; set; }
    public string? SsoUsername { get; set; }
    
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? Timezone { get; set; }
    public string Interests { get; set; } = "[]"; // JSON array
    public string Skills { get; set; } = "[]"; // JSON array
    
    public bool EncryptionEnabled { get; set; } = false;
    public int KeyRotationDays { get; set; } = 90;
    public DateTime? EncryptionEnabledAt { get; set; }
    public DateTime? EncryptionDisabledAt { get; set; }
    public DateTime? LastKeyRotation { get; set; }
    
    public bool DataCollectionConsent { get; set; } = false;
    public bool AnalyticsConsent { get; set; } = false;
    public bool MarketingConsent { get; set; } = false;
    public bool ThirdPartySharingConsent { get; set; } = false;
    public DateTime? LastConsentUpdate { get; set; }
    public bool DataDeletionRequested { get; set; } = false;
    public DateTime? DataDeletionRequestedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public virtual ICollection<Share> Shares { get; set; } = new List<Share>();
    public virtual ICollection<ChatTemplate> CreatedTemplates { get; set; } = new List<ChatTemplate>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<EncryptionKey> EncryptionKeys { get; set; } = new List<EncryptionKey>();
    public virtual UserRole? UserRole { get; set; }
}

public class Conversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty; // LM Studio model ID
    
    [MaxLength(50)]
    public string Category { get; set; } = "General"; // Conversation category/tag
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string ConversationId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsEncrypted { get; set; } = false;
    public string? EncryptionKeyId { get; set; }
    public string? Iv { get; set; }
    public string? Tag { get; set; }
    
    // Navigation Properties
    public virtual Conversation Conversation { get; set; } = null!;
    public virtual EncryptionKey? EncryptionKey { get; set; }
}

public class Share
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string ConversationId { get; set; } = string.Empty;
    
    [Required]
    public string SharedByUserId { get; set; } = string.Empty;
    
    public string? PasswordHash { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User SharedByUser { get; set; } = null!;
}

public class ChatTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    
    [Required]
    public string SystemPrompt { get; set; } = string.Empty;
    
    public string ExampleMessages { get; set; } = string.Empty; // JSON array of strings
    
    public bool IsBuiltIn { get; set; } = false;
    
    public string? CreatedByUserId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual User? CreatedByUser { get; set; }
}

public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;
    [Required]
    [MaxLength(100)]
    public string Resource { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    [MaxLength(45)]
    public string IpAddress { get; set; } = string.Empty;
    [MaxLength(500)]
    public string UserAgent { get; set; } = string.Empty;
    
    public virtual User? User { get; set; }
}

public class UserRole
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
    public string Permissions { get; set; } = string.Empty; // JSON array of permission strings
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsBuiltIn { get; set; } = false;
    
    // Navigation Properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

public class SsoConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    [MaxLength(50)]
    public string Provider { get; set; } = string.Empty;
    [Required]
    [MaxLength(200)]
    public string ServerUrl { get; set; } = string.Empty;
    [Required]
    [MaxLength(200)]
    public string BaseDn { get; set; } = string.Empty;
    [Required]
    [MaxLength(200)]
    public string BindDn { get; set; } = string.Empty;
    [Required]
    public string BindPassword { get; set; } = string.Empty;
    [Required]
    [MaxLength(500)]
    public string UserSearchFilter { get; set; } = string.Empty;
    [MaxLength(500)]
    public string GroupSearchFilter { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class UserPreferences
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string UserId { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Theme { get; set; } = "light";
    [MaxLength(10)]
    public string Language { get; set; } = "en";
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool DarkMode { get; set; } = false;
    public string NotificationSettings { get; set; } = "[]"; // JSON array
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
}

public class EncryptionKey
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Required]
    public string Key { get; set; } = string.Empty; // Base64 encoded AES-256 key
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeactivatedAt { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class DataExport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Required]
    public string DataTypes { get; set; } = string.Empty; // JSON array
    [Required]
    public string Format { get; set; } = "json";
    [Required]
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsDownloaded { get; set; } = false;
    public DateTime? DownloadedAt { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
}

public class DataDeletionRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Required]
    public string Reason { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    [Required]
    public string Status { get; set; } = "pending"; // pending, processing, completed, failed
    public string? AdminNotes { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
}

public class ConsentRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Required]
    public string ConsentType { get; set; } = string.Empty;
    public bool Granted { get; set; } = false;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    [Required]
    public string Purpose { get; set; } = string.Empty;
    public string? LegalBasis { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
}

public class Webhook
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Url { get; set; } = string.Empty;
    [Required]
    public string Secret { get; set; } = string.Empty;
    [Required]
    public string Events { get; set; } = string.Empty; // JSON array of event types
    public bool IsActive { get; set; } = true;
    public int RetryCount { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
    public string? CreatedBy { get; set; }
    
    // Navigation Properties
    public virtual ICollection<WebhookDelivery> Deliveries { get; set; } = new List<WebhookDelivery>();
}

public class WebhookDelivery
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string WebhookId { get; set; } = string.Empty;
    [Required]
    public string EventType { get; set; } = string.Empty;
    [Required]
    public string Payload { get; set; } = string.Empty; // JSON payload
    [Required]
    public string Status { get; set; } = "pending"; // pending, delivered, failed
    public int Attempts { get; set; } = 0;
    public int? ResponseCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? NextRetryAt { get; set; }
    
    // Navigation Properties
    public virtual Webhook Webhook { get; set; } = null!;
}
