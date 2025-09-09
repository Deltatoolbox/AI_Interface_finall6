namespace Gateway.Application.DTOs;

public record LoginRequest(string Username, string Password);

public record LoginResponse(bool Success, string? Token, string? Error);

public record CsrfResponse(string CsrfToken);

public record ChatRequest(
    string Model,
    IEnumerable<ChatMessageDto> Messages,
    double? Temperature = null,
    int? MaxTokens = null,
    double? TopP = null);

public record ChatMessageDto(string Role, string Content);

public record ConversationResponse(Guid Id, string Title, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateConversationRequest(string Title);

public record MessageResponse(
    Guid Id,
    string Role,
    string Content,
    int PromptTokens,
    int CompletionTokens,
    long LatencyMs,
    DateTime CreatedAt);

public record ConversationWithMessagesResponse(
    Guid Id,
    string Title,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<MessageResponse> Messages);

public record UsageLogResponse(
    Guid Id,
    string Model,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    DateTime CreatedAt);

public record AdminStatsResponse(
    int TotalUsers,
    int ActiveUsers,
    int TotalConversations,
    int TotalMessages,
    int ActiveStreams,
    Dictionary<string, int> ModelUsage);
