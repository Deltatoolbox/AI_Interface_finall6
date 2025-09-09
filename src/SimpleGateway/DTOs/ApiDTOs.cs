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

public record CreateConversationRequest(string Title);

public record UpdateConversationRequest(string Title);

public record ConversationResponse(string Id, string Title, DateTime CreatedAt, DateTime UpdatedAt);

public record MessageResponse(string Id, string Role, string Content, DateTime CreatedAt);

public record ConversationWithMessagesResponse(string Id, string Title, DateTime CreatedAt, DateTime UpdatedAt, MessageResponse[] Messages);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ResetPasswordRequest(string Username, string NewPassword);
