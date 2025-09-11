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
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public virtual ICollection<Share> Shares { get; set; } = new List<Share>();
    public virtual ICollection<ChatTemplate> CreatedTemplates { get; set; } = new List<ChatTemplate>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
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
    
    // Navigation Properties
    public virtual Conversation Conversation { get; set; } = null!;
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
