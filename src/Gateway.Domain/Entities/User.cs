using System.ComponentModel.DataAnnotations;

namespace Gateway.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "User";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    public int DailyTokenQuota { get; set; } = 100000;
    
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    
    public ICollection<UsageLog> UsageLogs { get; set; } = new List<UsageLog>();
}
