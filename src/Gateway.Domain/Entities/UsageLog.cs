using System.ComponentModel.DataAnnotations;

namespace Gateway.Domain.Entities;

public sealed class UsageLog
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;
    
    public int PromptTokens { get; set; }
    
    public int CompletionTokens { get; set; }
    
    public int TotalTokens { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public User User { get; set; } = null!;
}
