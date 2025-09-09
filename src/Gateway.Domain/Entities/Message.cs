using System.ComponentModel.DataAnnotations;

namespace Gateway.Domain.Entities;

public sealed class Message
{
    public Guid Id { get; set; }
    
    public Guid ConversationId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public int PromptTokens { get; set; }
    
    public int CompletionTokens { get; set; }
    
    public long LatencyMs { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Conversation Conversation { get; set; } = null!;
}
