using Microsoft.EntityFrameworkCore;
using SimpleGateway.Data;
using SimpleGateway.Models;
using SimpleGateway.DTOs;
using BCrypt.Net;

namespace SimpleGateway.Services;

public interface IUserService
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> CreateUserAsync(string username, string password, string email = "", string role = "User");
    Task<bool> ValidatePasswordAsync(User user, string password);
    Task<bool> ValidatePasswordAsync(string userId, string password);
    Task<List<User>> GetAllUsersAsync();
    Task<User?> UpdateUserAsync(string userId, string? username, string? email, string? role);
    Task<bool> UpdatePasswordAsync(string userId, string newPassword);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> UserExistsAsync(string username);
}

public interface IConversationService
{
    Task<ConversationResponse[]> GetConversationsByUserIdAsync(string userId);
    Task<ConversationResponse[]> GetAllConversationsForUserAsync(string userId);
    Task<ConversationResponse> CreateConversationAsync(string userId, string title, string model = "", string category = "General");
    Task<ConversationResponse> CreateConversationAsync(Conversation conversation);
    Task<ConversationWithMessagesResponse?> GetConversationWithMessagesAsync(string conversationId, string userId);
    Task<MessageResponse[]> GetMessagesByConversationIdAsync(string conversationId);
    Task<ConversationResponse?> UpdateConversationTitleAsync(string conversationId, string userId, string newTitle);
    Task<bool> DeleteAllConversationsForUserAsync(string userId);
    Task AddMessageAsync(Message message);
    Task<SearchResult[]> SearchMessagesAsync(string userId, string query, int? limit = null, int? offset = null);
}

public interface IShareService
{
    Task<ShareResponse> CreateShareAsync(string userId, string conversationId, string? password = null, DateTime? expiresAt = null);
    Task<SharedConversationResponse?> GetSharedConversationAsync(string shareId, string? password = null);
    Task<bool> RevokeShareAsync(string userId, string shareId);
    Task<ShareResponse[]> GetUserSharesAsync(string userId);
}

public interface IChatTemplateService
{
    Task<ChatTemplateDto[]> GetAllTemplatesAsync();
    Task<ChatTemplateDto[]> GetTemplatesByCategoryAsync(string category);
    Task<ChatTemplateDto?> GetTemplateByIdAsync(string templateId);
    Task<ChatTemplateDto> CreateTemplateAsync(string userId, CreateTemplateRequest request);
    Task<bool> UpdateTemplateAsync(string userId, string templateId, CreateTemplateRequest request);
    Task<bool> DeleteTemplateAsync(string userId, string templateId);
    Task<string[]> GetCategoriesAsync();
    Task SeedBuiltInTemplatesAsync();
}

public interface IBackupService
{
    Task<BackupInfo[]> GetBackupsAsync();
    Task<BackupInfo> CreateBackupAsync(string name, string? description = null);
    Task<bool> RestoreBackupAsync(string backupId);
    Task<bool> DeleteBackupAsync(string backupId);
    Task<byte[]> DownloadBackupAsync(string backupId);
    Task<bool> UploadBackupAsync(string name, string? description, byte[] backupData);
    Task ScheduleAutomaticBackupsAsync();
}

public interface IHealthMonitoringService
{
    Task<SystemHealth> GetSystemHealthAsync();
    Task<SystemMetrics> GetSystemMetricsAsync();
    Task<ServiceStatus[]> GetServiceStatusesAsync();
    Task<bool> CheckServiceHealthAsync(string serviceName);
    Task StartHealthMonitoringAsync();
    Task StopHealthMonitoringAsync();
}

public interface IMessageService
{
    Task SaveMessagesAsync(string conversationId, MessageDto[] messages);
    Task SaveAssistantMessageAsync(string conversationId, string content);
}

public class UserService : IUserService
{
    private readonly GatewayDbContext _context;

    public UserService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> CreateUserAsync(string username, string password, string email = "", string role = "User")
    {
        var existingUser = await GetUserByUsernameAsync(username);
        if (existingUser != null)
            return null;

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Email = email,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ValidatePasswordAsync(User user, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<bool> ValidatePasswordAsync(string userId, string password)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return false;
        
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<User?> UpdateUserAsync(string userId, string? username, string? email, string? role)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return null;

        if (!string.IsNullOrEmpty(username))
            user.Username = username;
        if (!string.IsNullOrEmpty(email))
            user.Email = email;
        if (!string.IsNullOrEmpty(role))
            user.Role = role;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username);
    }

    public async Task<bool> UpdatePasswordAsync(string userId, string newPassword)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}

public class ConversationService : IConversationService
{
    private readonly GatewayDbContext _context;

    public ConversationService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<ConversationResponse[]> GetConversationsByUserIdAsync(string userId)
    {
        var conversations = await _context.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new ConversationResponse(c.Id, c.Title, c.CreatedAt, c.UpdatedAt, c.Model, c.Category))
            .ToArrayAsync();

        return conversations;
    }

    public async Task<ConversationResponse> CreateConversationAsync(string userId, string title, string model = "", string category = "General")
    {
        var conversation = new Conversation
        {
            Title = title,
            UserId = userId,
            Model = model,
            Category = category,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        return new ConversationResponse(conversation.Id, conversation.Title, conversation.CreatedAt, conversation.UpdatedAt, conversation.Model, conversation.Category);
    }

    public async Task<ConversationWithMessagesResponse?> GetConversationWithMessagesAsync(string conversationId, string userId)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

        if (conversation == null)
            return null;

        var messages = conversation.Messages.Select(m => 
            new MessageResponse(m.Id, m.Role, m.Content, m.CreatedAt)).ToArray();

        return new ConversationWithMessagesResponse(
            conversation.Id, 
            conversation.Title, 
            conversation.CreatedAt, 
            conversation.UpdatedAt, 
            conversation.Model,
            conversation.Category,
            messages);
    }

    public async Task<ConversationResponse?> UpdateConversationTitleAsync(string conversationId, string userId, string newTitle)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

        if (conversation == null)
            return null;

        conversation.Title = newTitle;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ConversationResponse(conversation.Id, conversation.Title, conversation.CreatedAt, conversation.UpdatedAt, conversation.Model, conversation.Category);
    }

    public async Task<bool> DeleteAllConversationsForUserAsync(string userId)
    {
        var conversations = await _context.Conversations
            .Where(c => c.UserId == userId)
            .Include(c => c.Messages)
            .ToListAsync();

        if (!conversations.Any())
            return true;

        // Delete all messages first (due to foreign key constraints)
        foreach (var conversation in conversations)
        {
            _context.Messages.RemoveRange(conversation.Messages);
        }

        // Then delete conversations
        _context.Conversations.RemoveRange(conversations);
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ConversationResponse[]> GetAllConversationsForUserAsync(string userId)
    {
        return await GetConversationsByUserIdAsync(userId);
    }

    public async Task<ConversationResponse> CreateConversationAsync(Conversation conversation)
    {
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();
        return new ConversationResponse(conversation.Id, conversation.Title, conversation.CreatedAt, conversation.UpdatedAt, conversation.Model, conversation.Category);
    }

    public async Task<MessageResponse[]> GetMessagesByConversationIdAsync(string conversationId)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageResponse(m.Id, m.Role, m.Content, m.CreatedAt))
            .ToArrayAsync();

        return messages;
    }

    public async Task AddMessageAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
    }

    public async Task<SearchResult[]> SearchMessagesAsync(string userId, string query, int? limit = null, int? offset = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<SearchResult>();

        var baseQuery = _context.Messages
            .Where(m => m.Conversation.UserId == userId && m.Content.Contains(query))
            .Include(m => m.Conversation)
            .OrderByDescending(m => m.CreatedAt);

        var messages = await baseQuery
            .Skip(offset ?? 0)
            .Take(limit ?? 50)
            .ToArrayAsync();

        return messages.Select(m => 
        {
            // Simple highlighting - wrap matching text in <mark> tags
            var highlightedContent = HighlightText(m.Content, query);
            
            return new SearchResult(
                m.ConversationId,
                m.Conversation.Title,
                m.Id,
                m.Role,
                m.Content,
                m.CreatedAt,
                highlightedContent
            );
        }).ToArray();
    }

    private static string[] HighlightText(string content, string query)
    {
        if (string.IsNullOrEmpty(query))
            return new[] { content };

        var parts = content.Split(new[] { query }, StringSplitOptions.None);
        var result = new List<string>();
        
        for (int i = 0; i < parts.Length; i++)
        {
            if (i > 0)
                result.Add($"<mark>{query}</mark>");
            if (!string.IsNullOrEmpty(parts[i]))
                result.Add(parts[i]);
        }
        
        return result.ToArray();
    }
}

public class ShareService : IShareService
{
    private readonly GatewayDbContext _context;
    private readonly IConversationService _conversationService;

    public ShareService(GatewayDbContext context, IConversationService conversationService)
    {
        _context = context;
        _conversationService = conversationService;
    }

    public async Task<ShareResponse> CreateShareAsync(string userId, string conversationId, string? password = null, DateTime? expiresAt = null)
    {
        // Verify user owns the conversation
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);
        
        if (conversation == null)
            throw new UnauthorizedAccessException("Conversation not found or access denied");

        var share = new Share
        {
            ConversationId = conversationId,
            SharedByUserId = userId,
            PasswordHash = password != null ? BCrypt.Net.BCrypt.HashPassword(password) : null,
            ExpiresAt = expiresAt,
            IsActive = true
        };

        _context.Shares.Add(share);
        await _context.SaveChangesAsync();

        var shareUrl = $"http://localhost:5173/shared/{share.Id}";
        
        return new ShareResponse(
            share.Id,
            shareUrl,
            share.CreatedAt,
            share.ExpiresAt,
            !string.IsNullOrEmpty(share.PasswordHash)
        );
    }

    public async Task<SharedConversationResponse?> GetSharedConversationAsync(string shareId, string? password = null)
    {
        var share = await _context.Shares
            .Include(s => s.Conversation)
            .Include(s => s.SharedByUser)
            .FirstOrDefaultAsync(s => s.Id == shareId && s.IsActive);

        if (share == null)
            return null;

        // Check if expired
        if (share.ExpiresAt.HasValue && share.ExpiresAt.Value < DateTime.UtcNow)
            return null;

        // Check password if required
        if (!string.IsNullOrEmpty(share.PasswordHash))
        {
            if (string.IsNullOrEmpty(password) || !BCrypt.Net.BCrypt.Verify(password, share.PasswordHash))
                return null;
        }

        // Get conversation with messages
        var conversation = await _conversationService.GetConversationWithMessagesAsync(share.ConversationId, share.SharedByUserId);
        if (conversation == null)
            return null;

        return new SharedConversationResponse(
            conversation.Id,
            conversation.Title,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            conversation.Messages,
            share.SharedByUser.Username,
            share.CreatedAt
        );
    }

    public async Task<bool> RevokeShareAsync(string userId, string shareId)
    {
        var share = await _context.Shares
            .FirstOrDefaultAsync(s => s.Id == shareId && s.SharedByUserId == userId);

        if (share == null)
            return false;

        share.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ShareResponse[]> GetUserSharesAsync(string userId)
    {
        var shares = await _context.Shares
            .Where(s => s.SharedByUserId == userId && s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .ToArrayAsync();

        return shares.Select(s => new ShareResponse(
            s.Id,
            $"http://localhost:5173/shared/{s.Id}",
            s.CreatedAt,
            s.ExpiresAt,
            !string.IsNullOrEmpty(s.PasswordHash)
        )).ToArray();
    }
}

public class ChatTemplateService : IChatTemplateService
{
    private readonly GatewayDbContext _context;

    public ChatTemplateService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<ChatTemplateDto[]> GetAllTemplatesAsync()
    {
        var templates = await _context.ChatTemplates
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToArrayAsync();

        return templates.Select(MapToDto).ToArray();
    }

    public async Task<ChatTemplateDto[]> GetTemplatesByCategoryAsync(string category)
    {
        var templates = await _context.ChatTemplates
            .Where(t => t.Category == category)
            .OrderBy(t => t.Name)
            .ToArrayAsync();

        return templates.Select(MapToDto).ToArray();
    }

    public async Task<ChatTemplateDto?> GetTemplateByIdAsync(string templateId)
    {
        var template = await _context.ChatTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId);

        return template != null ? MapToDto(template) : null;
    }

    public async Task<ChatTemplateDto> CreateTemplateAsync(string userId, CreateTemplateRequest request)
    {
        var template = new ChatTemplate
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            SystemPrompt = request.SystemPrompt,
            ExampleMessages = System.Text.Json.JsonSerializer.Serialize(request.ExampleMessages),
            CreatedByUserId = userId,
            IsBuiltIn = false
        };

        _context.ChatTemplates.Add(template);
        await _context.SaveChangesAsync();
        return MapToDto(template);
    }

    public async Task<bool> UpdateTemplateAsync(string userId, string templateId, CreateTemplateRequest request)
    {
        var template = await _context.ChatTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId && t.CreatedByUserId == userId);

        if (template == null || template.IsBuiltIn)
            return false;

        template.Name = request.Name;
        template.Description = request.Description;
        template.Category = request.Category;
        template.SystemPrompt = request.SystemPrompt;
        template.ExampleMessages = System.Text.Json.JsonSerializer.Serialize(request.ExampleMessages);
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTemplateAsync(string userId, string templateId)
    {
        var template = await _context.ChatTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId && t.CreatedByUserId == userId);

        if (template == null || template.IsBuiltIn)
            return false;

        _context.ChatTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string[]> GetCategoriesAsync()
    {
        return await _context.ChatTemplates
            .Select(t => t.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToArrayAsync();
    }

    public async Task SeedBuiltInTemplatesAsync()
    {
        // Check if templates already exist
        if (await _context.ChatTemplates.AnyAsync())
            return;

        var builtInTemplates = new[]
        {
            new ChatTemplate
            {
                Id = "general-assistant",
                Name = "General Assistant",
                Description = "A helpful AI assistant for general questions and tasks",
                Category = "General",
                SystemPrompt = "You are a helpful AI assistant. Provide clear, accurate, and helpful responses to user questions.",
                ExampleMessages = System.Text.Json.JsonSerializer.Serialize(new[] { "Hello! How can I help you today?", "What would you like to know?" }),
                IsBuiltIn = true
            },
            new ChatTemplate
            {
                Id = "creative-writer",
                Name = "Creative Writer",
                Description = "Specialized in creative writing, storytelling, and content creation",
                Category = "Creative",
                SystemPrompt = "You are a creative writing assistant. Help users with storytelling, creative writing, brainstorming ideas, and developing characters and plots.",
                ExampleMessages = System.Text.Json.JsonSerializer.Serialize(new[] { "Help me write a short story about...", "Create a character profile for..." }),
                IsBuiltIn = true
            },
            new ChatTemplate
            {
                Id = "code-assistant",
                Name = "Code Assistant",
                Description = "Programming and software development helper",
                Category = "Programming",
                SystemPrompt = "You are a programming assistant. Help with code writing, debugging, explaining programming concepts, and best practices.",
                ExampleMessages = System.Text.Json.JsonSerializer.Serialize(new[] { "Help me write a function that...", "Explain this code:", "What's wrong with this code?" }),
                IsBuiltIn = true
            },
            new ChatTemplate
            {
                Id = "language-tutor",
                Name = "Language Tutor",
                Description = "Language learning and practice assistant",
                Category = "Education",
                SystemPrompt = "You are a language learning tutor. Help users practice languages, explain grammar, provide translations, and offer learning tips.",
                ExampleMessages = System.Text.Json.JsonSerializer.Serialize(new[] { "Help me practice Spanish", "Explain the difference between...", "Translate this text:" }),
                IsBuiltIn = true
            },
            new ChatTemplate
            {
                Id = "business-analyst",
                Name = "Business Analyst",
                Description = "Business strategy, analysis, and planning assistant",
                Category = "Business",
                SystemPrompt = "You are a business analyst assistant. Help with market analysis, business strategy, financial planning, and organizational development.",
                ExampleMessages = System.Text.Json.JsonSerializer.Serialize(new[] { "Analyze this business case:", "Help me create a business plan", "What are the risks of..." }),
                IsBuiltIn = true
            }
        };

        _context.ChatTemplates.AddRange(builtInTemplates);
        await _context.SaveChangesAsync();
    }

    private ChatTemplateDto MapToDto(ChatTemplate template)
    {
        var exampleMessages = string.IsNullOrEmpty(template.ExampleMessages) 
            ? Array.Empty<string>() 
            : System.Text.Json.JsonSerializer.Deserialize<string[]>(template.ExampleMessages) ?? Array.Empty<string>();

        return new ChatTemplateDto(
            template.Id,
            template.Name,
            template.Description,
            template.Category,
            template.SystemPrompt,
            exampleMessages,
            template.IsBuiltIn
        );
    }
}

public class BackupService : IBackupService
{
    private readonly GatewayDbContext _context;
    private readonly string _backupDirectory;
    private readonly Timer? _backupTimer;

    public BackupService(GatewayDbContext context)
    {
        _context = context;
        _backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "backups");
        
        // Ensure backup directory exists
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }

        // Schedule automatic backups every 24 hours
        _backupTimer = new Timer(async _ => await CreateAutomaticBackup(), null, TimeSpan.FromHours(24), TimeSpan.FromHours(24));
    }

    public async Task<BackupInfo[]> GetBackupsAsync()
    {
        var backupFiles = Directory.GetFiles(_backupDirectory, "*.db")
            .Select(filePath =>
            {
                var fileInfo = new FileInfo(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                return new BackupInfo(
                    fileName,
                    fileName,
                    fileInfo.CreationTime,
                    fileInfo.Length,
                    $"Backup created on {fileInfo.CreationTime:yyyy-MM-dd HH:mm}"
                );
            })
            .OrderByDescending(b => b.CreatedAt)
            .ToArray();

        return backupFiles;
    }

    public async Task<BackupInfo> CreateBackupAsync(string name, string? description = null)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"{name}_{timestamp}.db";
        var backupPath = Path.Combine(_backupDirectory, backupFileName);

        // Copy the current database file
        var sourceDbPath = Path.Combine(Directory.GetCurrentDirectory(), "gateway.db");
        if (File.Exists(sourceDbPath))
        {
            File.Copy(sourceDbPath, backupPath, true);
        }
        else
        {
            throw new FileNotFoundException("Source database file not found");
        }

        var fileInfo = new FileInfo(backupPath);
        return new BackupInfo(
            Path.GetFileNameWithoutExtension(backupFileName),
            name,
            fileInfo.CreationTime,
            fileInfo.Length,
            description ?? $"Manual backup created on {fileInfo.CreationTime:yyyy-MM-dd HH:mm}"
        );
    }

    public async Task<bool> RestoreBackupAsync(string backupId)
    {
        var backupPath = Path.Combine(_backupDirectory, $"{backupId}.db");
        if (!File.Exists(backupPath))
        {
            return false;
        }

        var targetDbPath = Path.Combine(Directory.GetCurrentDirectory(), "gateway.db");
        
        // Create a backup of current database before restoring
        var currentBackupPath = $"{targetDbPath}.backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        if (File.Exists(targetDbPath))
        {
            File.Copy(targetDbPath, currentBackupPath, true);
        }

        // Restore the backup
        File.Copy(backupPath, targetDbPath, true);
        
        return true;
    }

    public async Task<bool> DeleteBackupAsync(string backupId)
    {
        var backupPath = Path.Combine(_backupDirectory, $"{backupId}.db");
        if (!File.Exists(backupPath))
        {
            return false;
        }

        File.Delete(backupPath);
        return true;
    }

    public async Task<byte[]> DownloadBackupAsync(string backupId)
    {
        var backupPath = Path.Combine(_backupDirectory, $"{backupId}.db");
        if (!File.Exists(backupPath))
        {
            throw new FileNotFoundException("Backup file not found");
        }

        return await File.ReadAllBytesAsync(backupPath);
    }

    public async Task<bool> UploadBackupAsync(string name, string? description, byte[] backupData)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"{name}_{timestamp}.db";
        var backupPath = Path.Combine(_backupDirectory, backupFileName);

        await File.WriteAllBytesAsync(backupPath, backupData);
        return true;
    }

    public async Task ScheduleAutomaticBackupsAsync()
    {
        // This is handled by the timer in the constructor
        await Task.CompletedTask;
    }

    private async Task CreateAutomaticBackup()
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"auto_backup_{timestamp}.db";
            var backupPath = Path.Combine(_backupDirectory, backupFileName);

            var sourceDbPath = Path.Combine(Directory.GetCurrentDirectory(), "gateway.db");
            if (File.Exists(sourceDbPath))
            {
                File.Copy(sourceDbPath, backupPath, true);
                
                // Clean up old automatic backups (keep only last 7 days)
                await CleanupOldBackups();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating automatic backup: {ex.Message}");
        }
    }

    private async Task CleanupOldBackups()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-7);
            var oldBackups = Directory.GetFiles(_backupDirectory, "auto_backup_*.db")
                .Where(filePath =>
                {
                    var fileInfo = new FileInfo(filePath);
                    return fileInfo.CreationTime < cutoffDate;
                });

            foreach (var oldBackup in oldBackups)
            {
                File.Delete(oldBackup);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning up old backups: {ex.Message}");
        }
    }
}

public class HealthMonitoringService : IHealthMonitoringService
{
    private readonly GatewayDbContext _context;
    private readonly Timer? _monitoringTimer;
    private readonly Dictionary<string, ServiceStatus> _serviceStatuses;
    private readonly object _lock = new object();
    private int _totalRequests = 0;
    private readonly List<double> _responseTimes = new List<double>();

    public HealthMonitoringService(GatewayDbContext context)
    {
        _context = context;
        _serviceStatuses = new Dictionary<string, ServiceStatus>();
        
        // Initialize service statuses
        InitializeServiceStatuses();
        
        // Start monitoring timer (every 30 seconds)
        _monitoringTimer = new Timer(async _ => await UpdateSystemMetrics(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    private void InitializeServiceStatuses()
    {
        var services = new[]
        {
            "Database",
            "LM Studio API",
            "Authentication Service",
            "File Storage",
            "Backup Service"
        };

        foreach (var service in services)
        {
            _serviceStatuses[service] = new ServiceStatus(service, "Unknown", DateTime.UtcNow);
        }
    }

    public async Task<SystemHealth> GetSystemHealthAsync()
    {
        var metrics = await GetSystemMetricsAsync();
        var services = await GetServiceStatusesAsync();
        
        // Determine overall system health
        var overallStatus = DetermineOverallHealth(services);
        
        return new SystemHealth(overallStatus, DateTime.UtcNow, metrics, services);
    }

    public async Task<SystemMetrics> GetSystemMetricsAsync()
    {
        lock (_lock)
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var cpuUsage = GetCpuUsage();
            var memoryUsage = process.WorkingSet64;
            var diskUsage = GetDiskUsage();
            var activeConnections = GetActiveConnections();
            var avgResponseTime = _responseTimes.Any() ? _responseTimes.Average() : 0.0;

            return new SystemMetrics(
                cpuUsage,
                memoryUsage,
                diskUsage,
                activeConnections,
                _totalRequests,
                avgResponseTime
            );
        }
    }

    public async Task<ServiceStatus[]> GetServiceStatusesAsync()
    {
        lock (_lock)
        {
            return _serviceStatuses.Values.ToArray();
        }
    }

    public async Task<bool> CheckServiceHealthAsync(string serviceName)
    {
        try
        {
            var status = serviceName switch
            {
                "Database" => await CheckDatabaseHealth(),
                "LM Studio API" => await CheckLMStudioHealth(),
                "Authentication Service" => await CheckAuthServiceHealth(),
                "File Storage" => await CheckFileStorageHealth(),
                "Backup Service" => await CheckBackupServiceHealth(),
                _ => false
            };

            lock (_lock)
            {
                if (_serviceStatuses.ContainsKey(serviceName))
                {
                    _serviceStatuses[serviceName] = new ServiceStatus(
                        serviceName,
                        status ? "Healthy" : "Unhealthy",
                        DateTime.UtcNow,
                        status ? null : "Service check failed"
                    );
                }
            }

            return status;
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                if (_serviceStatuses.ContainsKey(serviceName))
                {
                    _serviceStatuses[serviceName] = new ServiceStatus(
                        serviceName,
                        "Error",
                        DateTime.UtcNow,
                        ex.Message
                    );
                }
            }
            return false;
        }
    }

    public async Task StartHealthMonitoringAsync()
    {
        // Start checking all services
        var tasks = _serviceStatuses.Keys.Select(CheckServiceHealthAsync);
        await Task.WhenAll(tasks);
    }

    public async Task StopHealthMonitoringAsync()
    {
        _monitoringTimer?.Dispose();
    }

    private async Task UpdateSystemMetrics()
    {
        try
        {
            await StartHealthMonitoringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating system metrics: {ex.Message}");
        }
    }

    private async Task<bool> CheckDatabaseHealth()
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckLMStudioHealth()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            var response = await httpClient.GetAsync("http://localhost:1234/v1/models");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckAuthServiceHealth()
    {
        // Check if JWT service is working
        return true; // Simplified check
    }

    private async Task<bool> CheckFileStorageHealth()
    {
        try
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "test");
            File.Delete(tempFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckBackupServiceHealth()
    {
        try
        {
            var backupDir = Path.Combine(Directory.GetCurrentDirectory(), "backups");
            return Directory.Exists(backupDir);
        }
        catch
        {
            return false;
        }
    }

    private double GetCpuUsage()
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            return process.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount;
        }
        catch
        {
            return 0.0;
        }
    }

    private long GetDiskUsage()
    {
        try
        {
            var drive = new DriveInfo(Directory.GetCurrentDirectory());
            return drive.TotalSize - drive.AvailableFreeSpace;
        }
        catch
        {
            return 0;
        }
    }

    private int GetActiveConnections()
    {
        try
        {
            // Simplified connection count
            return Environment.ProcessorCount;
        }
        catch
        {
            return 0;
        }
    }

    private string DetermineOverallHealth(ServiceStatus[] services)
    {
        var unhealthyServices = services.Count(s => s.Status != "Healthy");
        var totalServices = services.Length;

        if (unhealthyServices == 0)
            return "Healthy";
        else if (unhealthyServices <= totalServices / 2)
            return "Degraded";
        else
            return "Unhealthy";
    }

    public void RecordRequest(double responseTime)
    {
        lock (_lock)
        {
            _totalRequests++;
            _responseTimes.Add(responseTime);
            
            // Keep only last 100 response times
            if (_responseTimes.Count > 100)
            {
                _responseTimes.RemoveAt(0);
            }
        }
    }
}

public class MessageService : IMessageService
{
    private readonly GatewayDbContext _context;

    public MessageService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task SaveMessagesAsync(string conversationId, MessageDto[] messages)
    {
        var messageEntities = messages.Select(m => new Message
        {
            ConversationId = conversationId,
            Role = m.Role,
            Content = m.Content,
            CreatedAt = DateTime.UtcNow
        }).ToArray();

        _context.Messages.AddRange(messageEntities);

        // Update conversation timestamp
        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation != null)
        {
            conversation.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SaveAssistantMessageAsync(string conversationId, string content)
    {
        var message = new Message
        {
            ConversationId = conversationId,
            Role = "assistant",
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);

        // Update conversation timestamp
        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation != null)
        {
            conversation.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
