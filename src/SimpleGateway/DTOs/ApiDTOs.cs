namespace SimpleGateway.DTOs;

public record LoginRequest(string Username, string Password);

public record LoginResponse(bool Success, string? Token = null, string? Message = null, UserResponse? User = null);

public record UserResponse(string Id, string Username, string Email, string Role, DateTime CreatedAt);

public record RegisterRequest(string Username, string Password, string Email);

public record RegisterResponse(bool Success, string? Message = null, UserResponse? User = null);

public record CreateUserRequest(string Username, string Password, string Email, string Role);

public record UpdateUserRequest(string? Username, string? Email, string? Role);

public record ChatRequest(string Model, MessageDto[] Messages, string? ConversationId = null);

public record MessageDto(string Role, string Content);

public record CreateConversationRequest(string Title, string Model = "", string Category = "General");

public record UpdateConversationRequest(string Title);

public record ConversationResponse(string Id, string Title, DateTime CreatedAt, DateTime UpdatedAt, string Model, string Category);

public record MessageResponse(string Id, string Role, string Content, DateTime CreatedAt);

public record ConversationWithMessagesResponse(string Id, string Title, DateTime CreatedAt, DateTime UpdatedAt, string Model, string Category, MessageResponse[] Messages);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ResetPasswordRequest(string Username, string NewPassword);

public record ConversationExportData(string Id, string Title, DateTime CreatedAt, DateTime UpdatedAt, MessageExportData[] Messages);

public record MessageExportData(string Role, string Content, DateTime CreatedAt);

public record ConversationImportRequest(ConversationExportData[] Conversations);

public record SearchRequest(string Query, int? Limit = null, int? Offset = null);

public record SearchResult(string ConversationId, string ConversationTitle, string MessageId, string Role, string Content, DateTime CreatedAt, string[] HighlightedContent);

// Chat Sharing DTOs
public record CreateShareRequest(string ConversationId, string? Password = null, DateTime? ExpiresAt = null);
public record ShareResponse(string ShareId, string ShareUrl, DateTime CreatedAt, DateTime? ExpiresAt, bool IsPasswordProtected);
public record SharedConversationResponse(string Id, string Title, DateTime CreatedAt, DateTime UpdatedAt, MessageResponse[] Messages, string SharedBy, DateTime SharedAt);
public record ShareAccessRequest(string ShareId, string? Password = null);

// Chat Templates DTOs
public record ChatTemplateDto(string Id, string Name, string Description, string Category, string SystemPrompt, string[] ExampleMessages, bool IsBuiltIn);
public record CreateTemplateRequest(string Name, string Description, string Category, string SystemPrompt, string[] ExampleMessages);
public record ApplyTemplateRequest(string TemplateId, string? CustomPrompt = null);

// Backup/Restore DTOs
public record BackupInfo(string Id, string Name, DateTime CreatedAt, long SizeBytes, string Description);
public record CreateBackupRequest(string Name, string? Description = null);
public record RestoreBackupRequest(string BackupId);

// Health Monitoring DTOs
public record SystemHealth(string Status, DateTime Timestamp, SystemMetrics Metrics, ServiceStatus[] Services);
public record SystemMetrics(double CpuUsage, long MemoryUsage, long DiskUsage, int ActiveConnections, int TotalRequests, double ResponseTime);
public record ServiceStatus(string Name, string Status, DateTime LastCheck, string? ErrorMessage = null);
public record HealthCheckRequest(string ServiceName);

// Audit Trail DTOs
public record AuditLogDto(string Id, string UserId, string UserName, string Action, string Resource, string Details, DateTime Timestamp, string IpAddress, string UserAgent);
public record AuditLogFilter(string? UserId = null, string? Action = null, string? Resource = null, DateTime? StartDate = null, DateTime? EndDate = null, int Page = 1, int PageSize = 50);
public record AuditLogResponse(AuditLogDto[] Logs, int TotalCount, int Page, int PageSize, int TotalPages);

// User Roles DTOs
public record UserRoleDto(string Id, string Name, string Description, string[] Permissions, DateTime CreatedAt, DateTime UpdatedAt);
public record CreateUserRoleRequest(string Name, string Description, string[] Permissions);
public record UpdateUserRoleRequest(string Name, string Description, string[] Permissions);
public record AssignRoleRequest(string UserId, string RoleId);
public record UserWithRole(string Id, string Username, string Email, string Role, string RoleId, DateTime CreatedAt, DateTime LastLoginAt);
public record RolePermission(string Name, string Description, string Category);

// Guest Mode DTOs
public record GuestUser(string Id, string SessionId, DateTime CreatedAt, DateTime ExpiresAt, string IpAddress, string UserAgent, bool IsActive);
public record CreateGuestRequest(string SessionId, string IpAddress, string UserAgent);
public record GuestSession(string SessionId, string UserId, DateTime CreatedAt, DateTime ExpiresAt, bool IsActive);
public record GuestCleanupRequest(int MaxAgeHours = 24);
public record ConvertGuestRequest(string SessionId, string Username, string Password, string Email);
