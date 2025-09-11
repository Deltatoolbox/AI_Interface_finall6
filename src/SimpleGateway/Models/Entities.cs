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
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
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
