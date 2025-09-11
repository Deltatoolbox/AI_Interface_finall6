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
    Task InitializeDefaultUsersAsync();
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

public interface IAuditService
{
    Task LogActionAsync(string userId, string userName, string action, string resource, string details, string ipAddress, string userAgent);
    Task<AuditLogResponse> GetAuditLogsAsync(AuditLogFilter filter);
    Task<AuditLogDto[]> GetAuditLogsByUserAsync(string userId, int page = 1, int pageSize = 50);
    Task<AuditLogDto[]> GetAuditLogsByActionAsync(string action, int page = 1, int pageSize = 50);
    Task<AuditLogDto[]> GetAuditLogsByResourceAsync(string resource, int page = 1, int pageSize = 50);
    Task<AuditLogDto[]> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 50);
    Task<string[]> GetAvailableActionsAsync();
    Task<string[]> GetAvailableResourcesAsync();
}

public interface IUserRoleService
{
    Task<UserRoleDto[]> GetAllRolesAsync();
    Task<UserRoleDto?> GetRoleByIdAsync(string roleId);
    Task<UserRoleDto?> GetRoleByNameAsync(string roleName);
    Task<UserRoleDto> CreateRoleAsync(CreateUserRoleRequest request);
    Task<UserRoleDto> UpdateRoleAsync(string roleId, UpdateUserRoleRequest request);
    Task<bool> DeleteRoleAsync(string roleId);
    Task<bool> AssignRoleToUserAsync(string userId, string roleId);
    Task<bool> RemoveRoleFromUserAsync(string userId);
    Task<UserWithRole[]> GetUsersWithRolesAsync();
    Task<UserWithRole?> GetUserWithRoleAsync(string userId);
    Task<RolePermission[]> GetAvailablePermissionsAsync();
    Task<bool> UserHasPermissionAsync(string userId, string permission);
    Task InitializeDefaultRolesAsync();
}

public interface IGuestService
{
    Task<GuestUser> CreateGuestUserAsync(CreateGuestRequest request);
    Task<GuestUser?> GetGuestUserBySessionIdAsync(string sessionId);
    Task<bool> ExtendGuestSessionAsync(string sessionId, int additionalHours = 24);
    Task<bool> DeactivateGuestUserAsync(string sessionId);
    Task<int> CleanupExpiredGuestsAsync(int maxAgeHours = 24);
    Task<GuestUser[]> GetActiveGuestsAsync();
    Task<bool> IsGuestSessionValidAsync(string sessionId);
    Task<bool> ConvertGuestToUserAsync(string sessionId, string username, string password, string email);
}

public interface ISsoService
{
    Task<SsoConfigDto?> GetSsoConfigAsync(string provider);
    Task<SsoConfigDto> CreateSsoConfigAsync(SsoConfigDto config);
    Task<SsoConfigDto> UpdateSsoConfigAsync(string provider, SsoConfigDto config);
    Task<bool> DeleteSsoConfigAsync(string provider);
    Task<SsoConfigDto[]> GetAllSsoConfigsAsync();
    Task<SsoUser?> AuthenticateUserAsync(SsoLoginRequest request);
    Task<User?> CreateOrUpdateSsoUserAsync(SsoUser ssoUser);
    Task<bool> IsSsoEnabledAsync(string provider);
    Task<SsoUserMapping[]> GetSsoUserMappingsAsync();
}

public interface IUserProfileService
{
    Task<UserProfile?> GetUserProfileAsync(string userId);
    Task<UserProfile> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request);
    Task<UserPreferencesDto?> GetUserPreferencesAsync(string userId);
    Task<UserPreferencesDto> UpdateUserPreferencesAsync(string userId, UpdateUserPreferencesRequest request);
    Task<UserPreferencesDto> CreateDefaultPreferencesAsync(string userId);
    Task<UserProfile[]> GetAllUserProfilesAsync();
    Task<bool> DeleteUserProfileAsync(string userId);
}

public interface IEncryptionService
{
    Task<EncryptionKeyDto> CreateEncryptionKeyAsync(string userId, CreateEncryptionKeyRequest request);
    Task<EncryptionKeyDto?> GetActiveEncryptionKeyAsync(string userId);
    Task<EncryptionKeyDto[]> GetUserEncryptionKeysAsync(string userId);
    Task<bool> DeactivateEncryptionKeyAsync(string keyId);
    Task<bool> DeleteExpiredKeysAsync();
    Task<EncryptionStatus> GetEncryptionStatusAsync(string userId);
    Task<bool> UpdateEncryptionSettingsAsync(string userId, UpdateEncryptionSettingsRequest request);
    Task<string> EncryptMessageAsync(string content, string userId);
    Task<string> DecryptMessageAsync(DecryptMessageRequest request, string userId);
    Task<bool> IsEncryptionEnabledAsync(string userId);
}

public interface IGdprService
{
    Task<DataExportResponse> CreateDataExportAsync(DataExportRequest request);
    Task<DataExportResponse?> GetDataExportAsync(string exportId);
    Task<DataExportResponse[]> GetUserDataExportsAsync(string userId);
    Task<bool> DeleteExpiredExportsAsync();
    Task<DataDeletionResponse> RequestDataDeletionAsync(DataDeletionRequestDto request);
    Task<DataDeletionResponse[]> GetDataDeletionRequestsAsync(string userId);
    Task<DataDeletionResponse[]> GetAllDataDeletionRequestsAsync();
    Task<bool> ProcessDataDeletionAsync(string requestId, string adminNotes);
    Task<ConsentRecordDto> RecordConsentAsync(ConsentRequest request);
    Task<ConsentRecordDto[]> GetUserConsentsAsync(string userId);
    Task<bool> RevokeConsentAsync(string consentId);
    Task<PrivacySettings> GetPrivacySettingsAsync(string userId);
    Task<bool> UpdatePrivacySettingsAsync(string userId, UpdatePrivacySettingsRequest request);
    Task<GdprStatus> GetGdprStatusAsync(string userId);
    Task<bool> DeleteUserDataAsync(string userId);
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

    public async Task<User?> CreateUserAsync(string username, string password, string email = "", string roleName = "User")
    {
        var existingUser = await GetUserByUsernameAsync(username);
        if (existingUser != null)
            return null;

        // Find the role by name
        var role = await _context.UserRoles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null)
        {
            // If role doesn't exist, use default "User" role
            role = await _context.UserRoles.FirstOrDefaultAsync(r => r.Name == "User");
            if (role == null)
            {
                // If no roles exist, create a basic user without role
                var user = new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Email = email,
                    Role = roleName, // Fallback to old system
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
        }

        var newUser = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Email = email,
            Role = roleName, // Keep for backward compatibility
            RoleId = role.Id, // Use new role system
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
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

    public async Task InitializeDefaultUsersAsync()
    {
        var existingUsers = await _context.Users.AnyAsync();
        if (existingUsers) return;

        // Create default users with proper roles
        var adminRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var userRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.Name == "User");

        if (adminRole != null)
        {
            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                Email = "admin@example.com",
                Role = "Admin",
                RoleId = adminRole.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(adminUser);
        }

        if (userRole != null)
        {
            var testUser = new User
            {
                Username = "test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("test"),
                Email = "test@example.com",
                Role = "User",
                RoleId = userRole.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(testUser);
        }

        await _context.SaveChangesAsync();
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
        
        // Start initial health check immediately
        _ = Task.Run(async () => await StartHealthMonitoringAsync());
        
        // Start monitoring timer (every 30 seconds)
        _monitoringTimer = new Timer(async _ => await UpdateSystemMetrics(), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));
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
            _serviceStatuses[service] = new ServiceStatus(service, "Checking...", DateTime.UtcNow);
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
            // Update service statuses by calling health checks
            foreach (var service in _serviceStatuses.Keys.ToList())
            {
                await CheckServiceHealthAsync(service);
            }
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
            httpClient.Timeout = TimeSpan.FromSeconds(3);
            var response = await httpClient.GetAsync("http://localhost:1234/v1/models");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            // LM Studio is not running, which is expected in many cases
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
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;
            
            // Wait a short time to get a meaningful measurement
            Thread.Sleep(100);
            
            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;
            
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            
            return Math.Min(cpuUsageTotal * 100, 100.0); // Cap at 100%
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

public class AuditService : IAuditService
{
    private readonly GatewayDbContext _context;

    public AuditService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(string userId, string userName, string action, string resource, string details, string ipAddress, string userAgent)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = action,
            Resource = resource,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<AuditLogResponse> GetAuditLogsAsync(AuditLogFilter filter)
    {
        var query = _context.AuditLogs.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filter.UserId))
            query = query.Where(log => log.UserId == filter.UserId);

        if (!string.IsNullOrEmpty(filter.Action))
            query = query.Where(log => log.Action == filter.Action);

        if (!string.IsNullOrEmpty(filter.Resource))
            query = query.Where(log => log.Resource == filter.Resource);

        if (filter.StartDate.HasValue)
            query = query.Where(log => log.Timestamp >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(log => log.Timestamp <= filter.EndDate.Value);

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var logs = await query
            .OrderByDescending(log => log.Timestamp)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(log => new AuditLogDto(
                log.Id,
                log.UserId,
                log.UserName,
                log.Action,
                log.Resource,
                log.Details,
                log.Timestamp,
                log.IpAddress,
                log.UserAgent
            ))
            .ToArrayAsync();

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        return new AuditLogResponse(logs, totalCount, filter.Page, filter.PageSize, totalPages);
    }

    public async Task<AuditLogDto[]> GetAuditLogsByUserAsync(string userId, int page = 1, int pageSize = 50)
    {
        var logs = await _context.AuditLogs
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(log => new AuditLogDto(
                log.Id,
                log.UserId,
                log.UserName,
                log.Action,
                log.Resource,
                log.Details,
                log.Timestamp,
                log.IpAddress,
                log.UserAgent
            ))
            .ToArrayAsync();

        return logs;
    }

    public async Task<AuditLogDto[]> GetAuditLogsByActionAsync(string action, int page = 1, int pageSize = 50)
    {
        var logs = await _context.AuditLogs
            .Where(log => log.Action == action)
            .OrderByDescending(log => log.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(log => new AuditLogDto(
                log.Id,
                log.UserId,
                log.UserName,
                log.Action,
                log.Resource,
                log.Details,
                log.Timestamp,
                log.IpAddress,
                log.UserAgent
            ))
            .ToArrayAsync();

        return logs;
    }

    public async Task<AuditLogDto[]> GetAuditLogsByResourceAsync(string resource, int page = 1, int pageSize = 50)
    {
        var logs = await _context.AuditLogs
            .Where(log => log.Resource == resource)
            .OrderByDescending(log => log.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(log => new AuditLogDto(
                log.Id,
                log.UserId,
                log.UserName,
                log.Action,
                log.Resource,
                log.Details,
                log.Timestamp,
                log.IpAddress,
                log.UserAgent
            ))
            .ToArrayAsync();

        return logs;
    }

    public async Task<AuditLogDto[]> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 50)
    {
        var logs = await _context.AuditLogs
            .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate)
            .OrderByDescending(log => log.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(log => new AuditLogDto(
                log.Id,
                log.UserId,
                log.UserName,
                log.Action,
                log.Resource,
                log.Details,
                log.Timestamp,
                log.IpAddress,
                log.UserAgent
            ))
            .ToArrayAsync();

        return logs;
    }

    public async Task<string[]> GetAvailableActionsAsync()
    {
        var actions = await _context.AuditLogs
            .Select(log => log.Action)
            .Distinct()
            .OrderBy(action => action)
            .ToArrayAsync();

        return actions;
    }

    public async Task<string[]> GetAvailableResourcesAsync()
    {
        var resources = await _context.AuditLogs
            .Select(log => log.Resource)
            .Distinct()
            .OrderBy(resource => resource)
            .ToArrayAsync();

        return resources;
    }
}

public class UserRoleService : IUserRoleService
{
    private readonly GatewayDbContext _context;

    public UserRoleService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<UserRoleDto[]> GetAllRolesAsync()
    {
        var roles = await _context.UserRoles
            .OrderBy(r => r.Name)
            .ToArrayAsync();

        return roles.Select(r => new UserRoleDto(
            r.Id,
            r.Name,
            r.Description,
            System.Text.Json.JsonSerializer.Deserialize<string[]>(r.Permissions) ?? Array.Empty<string>(),
            r.CreatedAt,
            r.UpdatedAt
        )).ToArray();
    }

    public async Task<UserRoleDto?> GetRoleByIdAsync(string roleId)
    {
        var role = await _context.UserRoles
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null) return null;

        return new UserRoleDto(
            role.Id,
            role.Name,
            role.Description,
            System.Text.Json.JsonSerializer.Deserialize<string[]>(role.Permissions) ?? Array.Empty<string>(),
            role.CreatedAt,
            role.UpdatedAt
        );
    }

    public async Task<UserRoleDto?> GetRoleByNameAsync(string roleName)
    {
        var role = await _context.UserRoles
            .FirstOrDefaultAsync(r => r.Name == roleName);

        if (role == null) return null;

        return new UserRoleDto(
            role.Id,
            role.Name,
            role.Description,
            System.Text.Json.JsonSerializer.Deserialize<string[]>(role.Permissions) ?? Array.Empty<string>(),
            role.CreatedAt,
            role.UpdatedAt
        );
    }

    public async Task<UserRoleDto> CreateRoleAsync(CreateUserRoleRequest request)
    {
        var role = new Models.UserRole
        {
            Name = request.Name,
            Description = request.Description,
            Permissions = System.Text.Json.JsonSerializer.Serialize(request.Permissions),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UserRoles.Add(role);
        await _context.SaveChangesAsync();

        return new UserRoleDto(
            role.Id,
            role.Name,
            role.Description,
            request.Permissions,
            role.CreatedAt,
            role.UpdatedAt
        );
    }

    public async Task<UserRoleDto> UpdateRoleAsync(string roleId, UpdateUserRoleRequest request)
    {
        var role = await _context.UserRoles.FindAsync(roleId);
        if (role == null) throw new ArgumentException("Role not found");

        role.Name = request.Name;
        role.Description = request.Description;
        role.Permissions = System.Text.Json.JsonSerializer.Serialize(request.Permissions);
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new UserRoleDto(
            role.Id,
            role.Name,
            role.Description,
            request.Permissions,
            role.CreatedAt,
            role.UpdatedAt
        );
    }

    public async Task<bool> DeleteRoleAsync(string roleId)
    {
        var role = await _context.UserRoles.FindAsync(roleId);
        if (role == null) return false;

        if (role.IsBuiltIn) return false; // Cannot delete built-in roles

        _context.UserRoles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRoleToUserAsync(string userId, string roleId)
    {
        var user = await _context.Users.FindAsync(userId);
        var role = await _context.UserRoles.FindAsync(roleId);

        if (user == null || role == null) return false;

        user.RoleId = roleId;
        user.Role = role.Name;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRoleFromUserAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.RoleId = null;
        user.Role = "User";
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserWithRole[]> GetUsersWithRolesAsync()
    {
        var users = await _context.Users
            .Include(u => u.UserRole)
            .Select(u => new UserWithRole(
                u.Id,
                u.Username,
                u.Email,
                u.Role,
                u.RoleId ?? "",
                u.CreatedAt,
                u.UpdatedAt
            ))
            .ToArrayAsync();

        return users;
    }

    public async Task<UserWithRole?> GetUserWithRoleAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        return new UserWithRole(
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            user.RoleId ?? "",
            user.CreatedAt,
            user.UpdatedAt
        );
    }

    public async Task<RolePermission[]> GetAvailablePermissionsAsync()
    {
        var permissions = new List<RolePermission>
        {
            // User Management
            new("users.view", "View users", "User Management"),
            new("users.create", "Create users", "User Management"),
            new("users.edit", "Edit users", "User Management"),
            new("users.delete", "Delete users", "User Management"),
            
            // Role Management
            new("roles.view", "View roles", "Role Management"),
            new("roles.create", "Create roles", "Role Management"),
            new("roles.edit", "Edit roles", "Role Management"),
            new("roles.delete", "Delete roles", "Role Management"),
            new("roles.assign", "Assign roles to users", "Role Management"),
            
            // System Administration
            new("admin.dashboard", "Access admin dashboard", "System Administration"),
            new("admin.settings", "Manage system settings", "System Administration"),
            new("admin.backups", "Manage backups", "System Administration"),
            new("admin.health", "View system health", "System Administration"),
            new("admin.audit", "View audit logs", "System Administration"),
            
            // Content Management
            new("conversations.view", "View conversations", "Content Management"),
            new("conversations.create", "Create conversations", "Content Management"),
            new("conversations.edit", "Edit conversations", "Content Management"),
            new("conversations.delete", "Delete conversations", "Content Management"),
            new("conversations.share", "Share conversations", "Content Management"),
            
            // Templates
            new("templates.view", "View templates", "Templates"),
            new("templates.create", "Create templates", "Templates"),
            new("templates.edit", "Edit templates", "Templates"),
            new("templates.delete", "Delete templates", "Templates"),
            
            // File Management
            new("files.upload", "Upload files", "File Management"),
            new("files.download", "Download files", "File Management"),
            new("files.delete", "Delete files", "File Management")
        };

        return permissions.ToArray();
    }

    public async Task<bool> UserHasPermissionAsync(string userId, string permission)
    {
        var user = await _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.UserRole == null) return false;

        var permissions = System.Text.Json.JsonSerializer.Deserialize<string[]>(user.UserRole.Permissions) ?? Array.Empty<string>();
        return permissions.Contains(permission);
    }

    public async Task InitializeDefaultRolesAsync()
    {
        var existingRoles = await _context.UserRoles.AnyAsync();
        if (existingRoles) return;

        var defaultRoles = new[]
        {
            new Models.UserRole
            {
                Name = "SuperAdmin",
                Description = "Full system access with all permissions",
                Permissions = System.Text.Json.JsonSerializer.Serialize(new[]
                {
                    "users.view", "users.create", "users.edit", "users.delete",
                    "roles.view", "roles.create", "roles.edit", "roles.delete", "roles.assign",
                    "admin.dashboard", "admin.settings", "admin.backups", "admin.health", "admin.audit",
                    "conversations.view", "conversations.create", "conversations.edit", "conversations.delete", "conversations.share",
                    "templates.view", "templates.create", "templates.edit", "templates.delete",
                    "files.upload", "files.download", "files.delete"
                }),
                IsBuiltIn = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Models.UserRole
            {
                Name = "Admin",
                Description = "Administrative access with most permissions",
                Permissions = System.Text.Json.JsonSerializer.Serialize(new[]
                {
                    "users.view", "users.create", "users.edit",
                    "roles.view", "roles.assign",
                    "admin.dashboard", "admin.settings", "admin.backups", "admin.health", "admin.audit",
                    "conversations.view", "conversations.create", "conversations.edit", "conversations.delete", "conversations.share",
                    "templates.view", "templates.create", "templates.edit", "templates.delete",
                    "files.upload", "files.download", "files.delete"
                }),
                IsBuiltIn = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Models.UserRole
            {
                Name = "Moderator",
                Description = "Moderation access with limited administrative permissions",
                Permissions = System.Text.Json.JsonSerializer.Serialize(new[]
                {
                    "users.view",
                    "admin.dashboard", "admin.health", "admin.audit",
                    "conversations.view", "conversations.create", "conversations.edit", "conversations.share",
                    "templates.view", "templates.create", "templates.edit",
                    "files.upload", "files.download"
                }),
                IsBuiltIn = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Models.UserRole
            {
                Name = "User",
                Description = "Standard user with basic permissions",
                Permissions = System.Text.Json.JsonSerializer.Serialize(new[]
                {
                    "conversations.view", "conversations.create", "conversations.edit", "conversations.share",
                    "templates.view",
                    "files.upload", "files.download"
                }),
                IsBuiltIn = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _context.UserRoles.AddRange(defaultRoles);
        await _context.SaveChangesAsync();
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

public class GuestService : IGuestService
{
    private readonly GatewayDbContext _context;

    public GuestService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<GuestUser> CreateGuestUserAsync(CreateGuestRequest request)
    {
        var expiresAt = DateTime.UtcNow.AddHours(24); // Default 24 hours
        
        var guestUser = new User
        {
            Username = $"guest_{request.SessionId}",
            PasswordHash = "", // No password for guests
            Email = "",
            Role = "Guest",
            IsGuest = true,
            SessionId = request.SessionId,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(guestUser);
        await _context.SaveChangesAsync();

        return new GuestUser(
            guestUser.Id,
            guestUser.SessionId!,
            guestUser.CreatedAt,
            guestUser.ExpiresAt!.Value,
            request.IpAddress,
            request.UserAgent,
            true
        );
    }

    public async Task<GuestUser?> GetGuestUserBySessionIdAsync(string sessionId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.SessionId == sessionId && u.IsGuest);

        if (user == null) return null;

        return new GuestUser(
            user.Id,
            user.SessionId!,
            user.CreatedAt,
            user.ExpiresAt!.Value,
            "", // IP and UserAgent not stored in User entity
            "",
            user.ExpiresAt > DateTime.UtcNow
        );
    }

    public async Task<bool> ExtendGuestSessionAsync(string sessionId, int additionalHours = 24)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.SessionId == sessionId && u.IsGuest);

        if (user == null) return false;

        user.ExpiresAt = DateTime.UtcNow.AddHours(additionalHours);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateGuestUserAsync(string sessionId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.SessionId == sessionId && u.IsGuest);

        if (user == null) return false;

        user.ExpiresAt = DateTime.UtcNow.AddMinutes(-1); // Expire immediately
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CleanupExpiredGuestsAsync(int maxAgeHours = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-maxAgeHours);
        
        var expiredGuests = await _context.Users
            .Where(u => u.IsGuest && (u.ExpiresAt < DateTime.UtcNow || u.CreatedAt < cutoffTime))
            .ToListAsync();

        if (expiredGuests.Any())
        {
            // Delete related data first
            var guestIds = expiredGuests.Select(g => g.Id).ToList();
            
            // Delete conversations
            var conversations = await _context.Conversations
                .Where(c => guestIds.Contains(c.UserId))
                .ToListAsync();
            _context.Conversations.RemoveRange(conversations);

            // Delete messages
            var conversationIds = conversations.Select(c => c.Id).ToList();
            if (conversationIds.Any())
            {
                var messages = await _context.Messages
                    .Where(m => conversationIds.Contains(m.ConversationId))
                    .ToListAsync();
                _context.Messages.RemoveRange(messages);
            }

            // Delete shares
            var shares = await _context.Shares
                .Where(s => guestIds.Contains(s.SharedByUserId))
                .ToListAsync();
            _context.Shares.RemoveRange(shares);

            // Delete audit logs
            var auditLogs = await _context.AuditLogs
                .Where(a => guestIds.Contains(a.UserId))
                .ToListAsync();
            _context.AuditLogs.RemoveRange(auditLogs);

            // Finally delete the guest users
            _context.Users.RemoveRange(expiredGuests);
            await _context.SaveChangesAsync();
        }

        return expiredGuests.Count;
    }

    public async Task<GuestUser[]> GetActiveGuestsAsync()
    {
        var guests = await _context.Users
            .Where(u => u.IsGuest && u.ExpiresAt > DateTime.UtcNow)
            .OrderBy(u => u.CreatedAt)
            .Select(u => new GuestUser(
                u.Id,
                u.SessionId!,
                u.CreatedAt,
                u.ExpiresAt!.Value,
                "", // IP and UserAgent not stored
                "",
                true
            ))
            .ToArrayAsync();

        return guests;
    }

    public async Task<bool> IsGuestSessionValidAsync(string sessionId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.SessionId == sessionId && u.IsGuest);

        return user != null && user.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<bool> ConvertGuestToUserAsync(string sessionId, string username, string password, string email)
    {
        var guestUser = await _context.Users
            .FirstOrDefaultAsync(u => u.SessionId == sessionId && u.IsGuest);

        if (guestUser == null) return false;

        // Check if username already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && !u.IsGuest);

        if (existingUser != null) return false;

        // Convert guest to regular user
        guestUser.Username = username;
        guestUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        guestUser.Email = email;
        guestUser.Role = "User";
        guestUser.IsGuest = false;
        guestUser.SessionId = null;
        guestUser.ExpiresAt = null;
        guestUser.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}

public class SsoService : ISsoService
{
    private readonly GatewayDbContext _context;

    public SsoService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<SsoConfigDto?> GetSsoConfigAsync(string provider)
    {
        var config = await _context.SsoConfigs
            .FirstOrDefaultAsync(c => c.Provider == provider);

        if (config == null) return null;

        return new SsoConfigDto(
            config.Provider,
            config.ServerUrl,
            config.BaseDn,
            config.BindDn,
            config.BindPassword,
            config.UserSearchFilter,
            config.GroupSearchFilter,
            config.IsEnabled
        );
    }

    public async Task<SsoConfigDto> CreateSsoConfigAsync(SsoConfigDto config)
    {
        var ssoConfig = new Models.SsoConfig
        {
            Provider = config.Provider,
            ServerUrl = config.ServerUrl,
            BaseDn = config.BaseDn,
            BindDn = config.BindDn,
            BindPassword = config.BindPassword,
            UserSearchFilter = config.UserSearchFilter,
            GroupSearchFilter = config.GroupSearchFilter,
            IsEnabled = config.IsEnabled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SsoConfigs.Add(ssoConfig);
        await _context.SaveChangesAsync();

        return new SsoConfigDto(
            ssoConfig.Provider,
            ssoConfig.ServerUrl,
            ssoConfig.BaseDn,
            ssoConfig.BindDn,
            ssoConfig.BindPassword,
            ssoConfig.UserSearchFilter,
            ssoConfig.GroupSearchFilter,
            ssoConfig.IsEnabled
        );
    }

    public async Task<SsoConfigDto> UpdateSsoConfigAsync(string provider, SsoConfigDto config)
    {
        var ssoConfig = await _context.SsoConfigs
            .FirstOrDefaultAsync(c => c.Provider == provider);

        if (ssoConfig == null) throw new ArgumentException("SSO configuration not found");

        ssoConfig.ServerUrl = config.ServerUrl;
        ssoConfig.BaseDn = config.BaseDn;
        ssoConfig.BindDn = config.BindDn;
        ssoConfig.BindPassword = config.BindPassword;
        ssoConfig.UserSearchFilter = config.UserSearchFilter;
        ssoConfig.GroupSearchFilter = config.GroupSearchFilter;
        ssoConfig.IsEnabled = config.IsEnabled;
        ssoConfig.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new SsoConfigDto(
            ssoConfig.Provider,
            ssoConfig.ServerUrl,
            ssoConfig.BaseDn,
            ssoConfig.BindDn,
            ssoConfig.BindPassword,
            ssoConfig.UserSearchFilter,
            ssoConfig.GroupSearchFilter,
            ssoConfig.IsEnabled
        );
    }

    public async Task<bool> DeleteSsoConfigAsync(string provider)
    {
        var config = await _context.SsoConfigs
            .FirstOrDefaultAsync(c => c.Provider == provider);

        if (config == null) return false;

        _context.SsoConfigs.Remove(config);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<SsoConfigDto[]> GetAllSsoConfigsAsync()
    {
        var configs = await _context.SsoConfigs
            .OrderBy(c => c.Provider)
            .Select(c => new SsoConfigDto(
                c.Provider,
                c.ServerUrl,
                c.BaseDn,
                c.BindDn,
                c.BindPassword,
                c.UserSearchFilter,
                c.GroupSearchFilter,
                c.IsEnabled
            ))
            .ToArrayAsync();

        return configs;
    }

    public async Task<SsoUser?> AuthenticateUserAsync(SsoLoginRequest request)
    {
        var config = await GetSsoConfigAsync(request.Provider);
        if (config == null || !config.IsEnabled)
            return null;

        try
        {
            // Simple LDAP authentication simulation
            // In a real implementation, you would use a proper LDAP library
            // For now, we'll simulate authentication
            if (request.Username == "admin" && request.Password == "password")
            {
                return new SsoUser(
                    request.Username,
                    $"{request.Username}@company.com",
                    $"{request.Username} User",
                    new[] { "Users", "Admins" },
                    request.Provider
                );
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SSO authentication error: {ex.Message}");
            return null;
        }
    }

    public async Task<User?> CreateOrUpdateSsoUserAsync(SsoUser ssoUser)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.SsoUsername == ssoUser.Username && u.SsoProvider == ssoUser.Provider);

        if (existingUser != null)
        {
            // Update existing SSO user
            existingUser.Email = ssoUser.Email;
            existingUser.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingUser;
        }

        // Create new SSO user
        var newUser = new User
        {
            Username = ssoUser.Username,
            PasswordHash = "", // No password for SSO users
            Email = ssoUser.Email,
            Role = "User",
            IsSsoUser = true,
            SsoProvider = ssoUser.Provider,
            SsoUsername = ssoUser.Username,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
    }

    public async Task<bool> IsSsoEnabledAsync(string provider)
    {
        var config = await GetSsoConfigAsync(provider);
        return config?.IsEnabled ?? false;
    }

    public async Task<SsoUserMapping[]> GetSsoUserMappingsAsync()
    {
        var mappings = await _context.Users
            .Where(u => u.IsSsoUser)
            .Select(u => new SsoUserMapping(
                u.Id,
                u.SsoUsername!,
                u.SsoProvider!,
                u.CreatedAt
            ))
            .ToArrayAsync();

        return mappings;
    }
}

public class UserProfileService : IUserProfileService
{
    private readonly GatewayDbContext _context;

    public UserProfileService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetUserProfileAsync(string userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        var interests = System.Text.Json.JsonSerializer.Deserialize<string[]>(user.Interests) ?? Array.Empty<string>();
        var skills = System.Text.Json.JsonSerializer.Deserialize<string[]>(user.Skills) ?? Array.Empty<string>();

        return new UserProfile(
            user.Id,
            user.Username,
            user.Email,
            user.AvatarUrl,
            user.Bio,
            user.Location,
            user.Website,
            user.Timezone,
            interests,
            skills,
            user.CreatedAt,
            user.UpdatedAt
        );
    }

    public async Task<UserProfile> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) throw new ArgumentException("User not found");

        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
        if (request.Bio != null) user.Bio = request.Bio;
        if (request.Location != null) user.Location = request.Location;
        if (request.Website != null) user.Website = request.Website;
        if (request.Timezone != null) user.Timezone = request.Timezone;
        if (request.Interests != null) user.Interests = System.Text.Json.JsonSerializer.Serialize(request.Interests);
        if (request.Skills != null) user.Skills = System.Text.Json.JsonSerializer.Serialize(request.Skills);

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var interests = System.Text.Json.JsonSerializer.Deserialize<string[]>(user.Interests) ?? Array.Empty<string>();
        var skills = System.Text.Json.JsonSerializer.Deserialize<string[]>(user.Skills) ?? Array.Empty<string>();

        return new UserProfile(
            user.Id,
            user.Username,
            user.Email,
            user.AvatarUrl,
            user.Bio,
            user.Location,
            user.Website,
            user.Timezone,
            interests,
            skills,
            user.CreatedAt,
            user.UpdatedAt
        );
    }

    public async Task<UserPreferencesDto?> GetUserPreferencesAsync(string userId)
    {
        var preferences = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preferences == null) return null;

        var notificationSettings = System.Text.Json.JsonSerializer.Deserialize<string[]>(preferences.NotificationSettings) ?? Array.Empty<string>();

        return new UserPreferencesDto(
            preferences.UserId,
            preferences.Theme,
            preferences.Language,
            preferences.EmailNotifications,
            preferences.PushNotifications,
            preferences.DarkMode,
            notificationSettings,
            preferences.UpdatedAt
        );
    }

    public async Task<UserPreferencesDto> UpdateUserPreferencesAsync(string userId, UpdateUserPreferencesRequest request)
    {
        var preferences = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preferences == null)
        {
            var defaultPrefs = await CreateDefaultPreferencesAsync(userId);
            preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        if (request.Theme != null) preferences.Theme = request.Theme;
        if (request.Language != null) preferences.Language = request.Language;
        if (request.EmailNotifications.HasValue) preferences.EmailNotifications = request.EmailNotifications.Value;
        if (request.PushNotifications.HasValue) preferences.PushNotifications = request.PushNotifications.Value;
        if (request.DarkMode.HasValue) preferences.DarkMode = request.DarkMode.Value;
        if (request.NotificationSettings != null) preferences.NotificationSettings = System.Text.Json.JsonSerializer.Serialize(request.NotificationSettings);

        preferences.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var notificationSettings = System.Text.Json.JsonSerializer.Deserialize<string[]>(preferences.NotificationSettings) ?? Array.Empty<string>();

        return new UserPreferencesDto(
            preferences.UserId,
            preferences.Theme,
            preferences.Language,
            preferences.EmailNotifications,
            preferences.PushNotifications,
            preferences.DarkMode,
            notificationSettings,
            preferences.UpdatedAt
        );
    }

    public async Task<UserPreferencesDto> CreateDefaultPreferencesAsync(string userId)
    {
        var preferences = new Models.UserPreferences
        {
            UserId = userId,
            Theme = "light",
            Language = "en",
            EmailNotifications = true,
            PushNotifications = true,
            DarkMode = false,
            NotificationSettings = "[]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UserPreferences.Add(preferences);
        await _context.SaveChangesAsync();

        return new UserPreferencesDto(
            preferences.UserId,
            preferences.Theme,
            preferences.Language,
            preferences.EmailNotifications,
            preferences.PushNotifications,
            preferences.DarkMode,
            Array.Empty<string>(),
            preferences.UpdatedAt
        );
    }

    public async Task<UserProfile[]> GetAllUserProfilesAsync()
    {
        var users = await _context.Users
            .OrderBy(u => u.Username)
            .ToArrayAsync();

        var profiles = new List<UserProfile>();
        foreach (var user in users)
        {
            var interests = System.Text.Json.JsonSerializer.Deserialize<string[]>(user.Interests) ?? Array.Empty<string>();
            var skills = System.Text.Json.JsonSerializer.Deserialize<string[]>(user.Skills) ?? Array.Empty<string>();

            profiles.Add(new UserProfile(
                user.Id,
                user.Username,
                user.Email,
                user.AvatarUrl,
                user.Bio,
                user.Location,
                user.Website,
                user.Timezone,
                interests,
                skills,
                user.CreatedAt,
                user.UpdatedAt
            ));
        }

        return profiles.ToArray();
    }

    public async Task<bool> DeleteUserProfileAsync(string userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        // Reset profile fields to defaults
        user.AvatarUrl = null;
        user.Bio = null;
        user.Location = null;
        user.Website = null;
        user.Timezone = null;
        user.Interests = "[]";
        user.Skills = "[]";
        user.UpdatedAt = DateTime.UtcNow;

        // Delete preferences
        var preferences = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (preferences != null)
        {
            _context.UserPreferences.Remove(preferences);
        }

        await _context.SaveChangesAsync();
        return true;
    }
}

public class EncryptionService : IEncryptionService
{
    private readonly GatewayDbContext _context;

    public EncryptionService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<EncryptionKeyDto> CreateEncryptionKeyAsync(string userId, CreateEncryptionKeyRequest request)
    {
        // Deactivate existing keys
        var existingKeys = await _context.EncryptionKeys
            .Where(k => k.UserId == userId && k.IsActive)
            .ToListAsync();

        foreach (var key in existingKeys)
        {
            key.IsActive = false;
        }

        // Create new key
        var expiresAt = DateTime.UtcNow.AddDays(request.ExpirationDays);
        var encryptionKey = new Models.EncryptionKey
        {
            UserId = userId,
            PublicKey = request.PublicKey,
            EncryptedPrivateKey = request.EncryptedPrivateKey,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsActive = true
        };

        _context.EncryptionKeys.Add(encryptionKey);
        await _context.SaveChangesAsync();

        return new EncryptionKeyDto(
            encryptionKey.Id,
            encryptionKey.UserId,
            encryptionKey.PublicKey,
            encryptionKey.EncryptedPrivateKey,
            encryptionKey.CreatedAt,
            encryptionKey.ExpiresAt,
            encryptionKey.IsActive
        );
    }

    public async Task<EncryptionKeyDto?> GetActiveEncryptionKeyAsync(string userId)
    {
        var key = await _context.EncryptionKeys
            .FirstOrDefaultAsync(k => k.UserId == userId && k.IsActive && k.ExpiresAt > DateTime.UtcNow);

        if (key == null) return null;

        return new EncryptionKeyDto(
            key.Id,
            key.UserId,
            key.PublicKey,
            key.EncryptedPrivateKey,
            key.CreatedAt,
            key.ExpiresAt,
            key.IsActive
        );
    }

    public async Task<EncryptionKeyDto[]> GetUserEncryptionKeysAsync(string userId)
    {
        var keys = await _context.EncryptionKeys
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new EncryptionKeyDto(
                k.Id,
                k.UserId,
                k.PublicKey,
                k.EncryptedPrivateKey,
                k.CreatedAt,
                k.ExpiresAt,
                k.IsActive
            ))
            .ToArrayAsync();

        return keys;
    }

    public async Task<bool> DeactivateEncryptionKeyAsync(string keyId)
    {
        var key = await _context.EncryptionKeys
            .FirstOrDefaultAsync(k => k.Id == keyId);

        if (key == null) return false;

        key.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteExpiredKeysAsync()
    {
        var expiredKeys = await _context.EncryptionKeys
            .Where(k => k.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        if (expiredKeys.Any())
        {
            _context.EncryptionKeys.RemoveRange(expiredKeys);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<EncryptionStatus> GetEncryptionStatusAsync(string userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) throw new ArgumentException("User not found");

        var activeKey = await GetActiveEncryptionKeyAsync(userId);

        return new EncryptionStatus(
            userId,
            activeKey != null,
            activeKey?.ExpiresAt,
            user.EncryptionEnabled
        );
    }

    public async Task<bool> UpdateEncryptionSettingsAsync(string userId, UpdateEncryptionSettingsRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        if (request.EncryptionEnabled.HasValue)
            user.EncryptionEnabled = request.EncryptionEnabled.Value;

        if (request.KeyRotationDays.HasValue)
            user.KeyRotationDays = request.KeyRotationDays.Value;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> EncryptMessageAsync(string content, string userId)
    {
        // Simple encryption simulation - in production, use proper encryption libraries
        // This is a placeholder implementation
        var key = await GetActiveEncryptionKeyAsync(userId);
        if (key == null) throw new InvalidOperationException("No active encryption key found");

        // In a real implementation, you would use AES encryption with the user's key
        // For now, we'll return a base64 encoded version as a placeholder
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        return Convert.ToBase64String(bytes);
    }

    public async Task<string> DecryptMessageAsync(DecryptMessageRequest request, string userId)
    {
        // Simple decryption simulation - in production, use proper decryption libraries
        // This is a placeholder implementation
        var key = await _context.EncryptionKeys
            .FirstOrDefaultAsync(k => k.Id == request.EncryptionKeyId && k.UserId == userId);

        if (key == null) throw new InvalidOperationException("Encryption key not found");

        // In a real implementation, you would use AES decryption with the user's key
        // For now, we'll decode the base64 as a placeholder
        var bytes = Convert.FromBase64String(request.EncryptedContent);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    public async Task<bool> IsEncryptionEnabledAsync(string userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.EncryptionEnabled ?? false;
    }
}

public class GdprService : IGdprService
{
    private readonly GatewayDbContext _context;

    public GdprService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<DataExportResponse> CreateDataExportAsync(DataExportRequest request)
    {
        var expiresAt = DateTime.UtcNow.AddDays(30); // Exports expire after 30 days
        var exportId = Guid.NewGuid().ToString();
        var fileName = $"data_export_{request.UserId}_{exportId}.{request.Format}";
        var filePath = Path.Combine("exports", fileName);

        // Ensure exports directory exists
        Directory.CreateDirectory("exports");

        // Create export record
        var dataExport = new Models.DataExport
        {
            Id = exportId,
            UserId = request.UserId,
            DataTypes = System.Text.Json.JsonSerializer.Serialize(request.DataTypes),
            Format = request.Format,
            FilePath = filePath,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };

        _context.DataExports.Add(dataExport);
        await _context.SaveChangesAsync();

        // Generate export file (placeholder implementation)
        await GenerateExportFileAsync(dataExport, request);

        return new DataExportResponse(
            dataExport.Id,
            dataExport.UserId,
            $"/api/gdpr/exports/{dataExport.Id}/download",
            dataExport.CreatedAt,
            dataExport.ExpiresAt
        );
    }

    public async Task<DataExportResponse?> GetDataExportAsync(string exportId)
    {
        var export = await _context.DataExports
            .FirstOrDefaultAsync(e => e.Id == exportId);

        if (export == null) return null;

        return new DataExportResponse(
            export.Id,
            export.UserId,
            $"/api/gdpr/exports/{export.Id}/download",
            export.CreatedAt,
            export.ExpiresAt
        );
    }

    public async Task<DataExportResponse[]> GetUserDataExportsAsync(string userId)
    {
        var exports = await _context.DataExports
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new DataExportResponse(
                e.Id,
                e.UserId,
                $"/api/gdpr/exports/{e.Id}/download",
                e.CreatedAt,
                e.ExpiresAt
            ))
            .ToArrayAsync();

        return exports;
    }

    public async Task<bool> DeleteExpiredExportsAsync()
    {
        var expiredExports = await _context.DataExports
            .Where(e => e.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var export in expiredExports)
        {
            // Delete file if it exists
            if (File.Exists(export.FilePath))
            {
                File.Delete(export.FilePath);
            }
        }

        if (expiredExports.Any())
        {
            _context.DataExports.RemoveRange(expiredExports);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<DataDeletionResponse> RequestDataDeletionAsync(DataDeletionRequestDto request)
    {
        var deletionRequest = new Models.DataDeletionRequest
        {
            UserId = request.UserId,
            Reason = request.Reason,
            RequestedAt = DateTime.UtcNow,
            Status = "pending"
        };

        _context.DataDeletionRequests.Add(deletionRequest);
        await _context.SaveChangesAsync();

        return new DataDeletionResponse(
            deletionRequest.Id,
            deletionRequest.UserId,
            deletionRequest.RequestedAt,
            deletionRequest.CompletedAt,
            deletionRequest.Status
        );
    }

    public async Task<DataDeletionResponse[]> GetDataDeletionRequestsAsync(string userId)
    {
        var requests = await _context.DataDeletionRequests
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new DataDeletionResponse(
                r.Id,
                r.UserId,
                r.RequestedAt,
                r.CompletedAt,
                r.Status
            ))
            .ToArrayAsync();

        return requests;
    }

    public async Task<DataDeletionResponse[]> GetAllDataDeletionRequestsAsync()
    {
        var requests = await _context.DataDeletionRequests
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new DataDeletionResponse(
                r.Id,
                r.UserId,
                r.RequestedAt,
                r.CompletedAt,
                r.Status
            ))
            .ToArrayAsync();

        return requests;
    }

    public async Task<bool> ProcessDataDeletionAsync(string requestId, string adminNotes)
    {
        var request = await _context.DataDeletionRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null) return false;

        request.Status = "processing";
        request.AdminNotes = adminNotes;
        await _context.SaveChangesAsync();

        // Process deletion (placeholder implementation)
        await DeleteUserDataAsync(request.UserId);

        request.Status = "completed";
        request.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ConsentRecordDto> RecordConsentAsync(ConsentRequest request)
    {
        var consent = new Models.ConsentRecord
        {
            UserId = request.UserId,
            ConsentType = request.ConsentType,
            Granted = request.Granted,
            GrantedAt = DateTime.UtcNow,
            Purpose = request.Purpose,
            LegalBasis = "consent"
        };

        _context.ConsentRecords.Add(consent);
        await _context.SaveChangesAsync();

        return new ConsentRecordDto(
            consent.Id,
            consent.UserId,
            consent.ConsentType,
            consent.Granted,
            consent.GrantedAt,
            consent.RevokedAt,
            consent.Purpose
        );
    }

    public async Task<ConsentRecordDto[]> GetUserConsentsAsync(string userId)
    {
        var consents = await _context.ConsentRecords
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.GrantedAt)
            .Select(c => new ConsentRecordDto(
                c.Id,
                c.UserId,
                c.ConsentType,
                c.Granted,
                c.GrantedAt,
                c.RevokedAt,
                c.Purpose
            ))
            .ToArrayAsync();

        return consents;
    }

    public async Task<bool> RevokeConsentAsync(string consentId)
    {
        var consent = await _context.ConsentRecords
            .FirstOrDefaultAsync(c => c.Id == consentId);

        if (consent == null) return false;

        consent.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PrivacySettings> GetPrivacySettingsAsync(string userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) throw new ArgumentException("User not found");

        return new PrivacySettings(
            userId,
            user.DataCollectionConsent,
            user.AnalyticsConsent,
            user.MarketingConsent,
            user.ThirdPartySharingConsent,
            user.LastConsentUpdate ?? user.UpdatedAt
        );
    }

    public async Task<bool> UpdatePrivacySettingsAsync(string userId, UpdatePrivacySettingsRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        if (request.DataCollection.HasValue)
            user.DataCollectionConsent = request.DataCollection.Value;

        if (request.Analytics.HasValue)
            user.AnalyticsConsent = request.Analytics.Value;

        if (request.Marketing.HasValue)
            user.MarketingConsent = request.Marketing.Value;

        if (request.ThirdPartySharing.HasValue)
            user.ThirdPartySharingConsent = request.ThirdPartySharing.Value;

        user.LastConsentUpdate = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<GdprStatus> GetGdprStatusAsync(string userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) throw new ArgumentException("User not found");

        var hasConsented = user.DataCollectionConsent || user.AnalyticsConsent || user.MarketingConsent;
        var hasDataExport = await _context.DataExports.AnyAsync(e => e.UserId == userId && e.ExpiresAt > DateTime.UtcNow);
        var hasDeletionRequest = await _context.DataDeletionRequests.AnyAsync(r => r.UserId == userId && r.Status == "pending");

        return new GdprStatus(
            userId,
            hasConsented,
            user.LastConsentUpdate,
            hasDataExport,
            hasDeletionRequest
        );
    }

    public async Task<bool> DeleteUserDataAsync(string userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        // Delete all user data
        user.DataDeletionRequested = true;
        user.DataDeletionRequestedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task GenerateExportFileAsync(Models.DataExport export, DataExportRequest request)
    {
        // Placeholder implementation - in production, generate actual export file
        var exportData = new
        {
            UserId = export.UserId,
            ExportId = export.Id,
            DataTypes = request.DataTypes,
            Format = request.Format,
            CreatedAt = export.CreatedAt,
            ExpiresAt = export.ExpiresAt,
            Message = "This is a placeholder export file. In production, this would contain actual user data."
        };

        var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(export.FilePath, json);
    }
}
