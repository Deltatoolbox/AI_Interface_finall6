using System.ComponentModel.DataAnnotations;

namespace Gateway.Domain.Entities;

public sealed class Conversation
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public User User { get; set; } = null!;
    
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
