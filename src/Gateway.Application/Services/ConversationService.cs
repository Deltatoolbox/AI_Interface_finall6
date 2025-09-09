using Gateway.Application.DTOs;
using Gateway.Domain.Entities;
using Gateway.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gateway.Application.Services;

public sealed class ConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ILogger<ConversationService> logger)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new conversation for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Conversation creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created conversation response</returns>
    public async Task<ConversationResponse> CreateConversationAsync(
        Guid userId,
        CreateConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdConversation = await _conversationRepository.CreateAsync(conversation, cancellationToken);
        
        _logger.LogInformation("Created conversation {ConversationId} for user {UserId}", 
            createdConversation.Id, userId);

        return new ConversationResponse(
            createdConversation.Id,
            createdConversation.Title,
            createdConversation.CreatedAt,
            createdConversation.UpdatedAt);
    }

    /// <summary>
    /// Gets conversations for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of conversations</returns>
    public async Task<IEnumerable<ConversationResponse>> GetConversationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.GetByUserIdAsync(userId, cancellationToken);
        
        return conversations.Select(c => new ConversationResponse(
            c.Id,
            c.Title,
            c.CreatedAt,
            c.UpdatedAt));
    }

    /// <summary>
    /// Gets a conversation with its messages
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="userId">User ID for authorization</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversation with messages</returns>
    public async Task<ConversationWithMessagesResponse?> GetConversationWithMessagesAsync(
        Guid conversationId,
        Guid userId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);
        
        if (conversation == null || conversation.UserId != userId)
        {
            return null;
        }

        var messages = await _messageRepository.GetByConversationIdAsync(
            conversationId, page, pageSize, cancellationToken);

        var messageResponses = messages.Select(m => new MessageResponse(
            m.Id,
            m.Role,
            m.Content,
            m.PromptTokens,
            m.CompletionTokens,
            m.LatencyMs,
            m.CreatedAt));

        return new ConversationWithMessagesResponse(
            conversation.Id,
            conversation.Title,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            messageResponses);
    }
}
