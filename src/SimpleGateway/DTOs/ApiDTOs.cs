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
