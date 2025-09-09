using Gateway.Application.DTOs;
using Gateway.Domain.Entities;
using Gateway.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gateway.Application.Services;

public sealed class ChatService
{
    private readonly ILmStudioClient _lmStudioClient;
    private readonly IMessageRepository _messageRepository;
    private readonly IUsageLogRepository _usageLogRepository;
    private readonly IConcurrencyManager _concurrencyManager;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        ILmStudioClient lmStudioClient,
        IMessageRepository messageRepository,
        IUsageLogRepository usageLogRepository,
        IConcurrencyManager concurrencyManager,
        ILogger<ChatService> logger)
    {
        _lmStudioClient = lmStudioClient;
        _messageRepository = messageRepository;
        _usageLogRepository = usageLogRepository;
        _concurrencyManager = concurrencyManager;
        _logger = logger;
    }

    /// <summary>
    /// Processes a chat request and returns streaming response
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="request">Chat request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Streaming response</returns>
    public async Task<Stream> ProcessChatAsync(
        Guid userId,
        Guid conversationId,
        ChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        if (!await _concurrencyManager.TryAcquireUserStreamSlotAsync(userId, cancellationToken))
        {
            throw new InvalidOperationException("User has reached maximum active streams limit");
        }

        try
        {
            using var modelSemaphore = await _concurrencyManager.AcquireModelSemaphoreAsync(request.Model, cancellationToken);
            
            var lmRequest = new ChatCompletionRequest(
                request.Model,
                request.Messages.Select(m => new ChatMessage(m.Role, m.Content)),
                request.Temperature,
                request.MaxTokens,
                request.TopP,
                true);

            var responseStream = await _lmStudioClient.ChatCompletionStreamAsync(lmRequest, cancellationToken);
            
            return new ChatResponseStream(
                responseStream,
                userId,
                conversationId,
                request,
                _messageRepository,
                _usageLogRepository,
                _concurrencyManager,
                startTime,
                _logger);
        }
        catch
        {
            _concurrencyManager.ReleaseUserStreamSlot(userId);
            throw;
        }
    }

    /// <summary>
    /// Gets available models from LM Studio
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available models</returns>
    public async Task<IEnumerable<LmModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        return await _lmStudioClient.GetModelsAsync(cancellationToken);
    }
}

internal sealed class ChatResponseStream : Stream
{
    private readonly Stream _innerStream;
    private readonly Guid _userId;
    private readonly Guid _conversationId;
    private readonly ChatRequest _request;
    private readonly IMessageRepository _messageRepository;
    private readonly IUsageLogRepository _usageLogRepository;
    private readonly IConcurrencyManager _concurrencyManager;
    private readonly DateTime _startTime;
    private readonly ILogger _logger;
    private readonly MemoryStream _contentBuffer = new();
    private bool _disposed = false;
    private int _promptTokens = 0;
    private int _completionTokens = 0;

    public ChatResponseStream(
        Stream innerStream,
        Guid userId,
        Guid conversationId,
        ChatRequest request,
        IMessageRepository messageRepository,
        IUsageLogRepository usageLogRepository,
        IConcurrencyManager concurrencyManager,
        DateTime startTime,
        ILogger logger)
    {
        _innerStream = innerStream;
        _userId = userId;
        _conversationId = conversationId;
        _request = request;
        _messageRepository = messageRepository;
        _usageLogRepository = usageLogRepository;
        _concurrencyManager = concurrencyManager;
        _startTime = startTime;
        _logger = logger;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get; set; }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var bytesRead = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        
        if (bytesRead > 0)
        {
            await _contentBuffer.WriteAsync(buffer, offset, bytesRead, cancellationToken);
        }
        
        return bytesRead;
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                SaveMessagesAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving messages for conversation {ConversationId}", _conversationId);
            }
            finally
            {
                _concurrencyManager.ReleaseUserStreamSlot(_userId);
                _innerStream.Dispose();
                _contentBuffer.Dispose();
                _disposed = true;
            }
        }
        
        base.Dispose(disposing);
    }

    private async Task SaveMessagesAsync()
    {
        var latency = (DateTime.UtcNow - _startTime).TotalMilliseconds;
        
        var userMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = _conversationId,
            Role = "user",
            Content = _request.Messages.Last().Content,
            PromptTokens = _promptTokens,
            CompletionTokens = 0,
            LatencyMs = 0,
            CreatedAt = DateTime.UtcNow
        };

        var assistantMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = _conversationId,
            Role = "assistant",
            Content = await GetContentFromBuffer(),
            PromptTokens = 0,
            CompletionTokens = _completionTokens,
            LatencyMs = (long)latency,
            CreatedAt = DateTime.UtcNow
        };

        await _messageRepository.CreateAsync(userMessage);
        await _messageRepository.CreateAsync(assistantMessage);

        var usageLog = new UsageLog
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Model = _request.Model,
            PromptTokens = _promptTokens,
            CompletionTokens = _completionTokens,
            TotalTokens = _promptTokens + _completionTokens,
            CreatedAt = DateTime.UtcNow
        };

        await _usageLogRepository.CreateAsync(usageLog);
    }

    private async Task<string> GetContentFromBuffer()
    {
        _contentBuffer.Position = 0;
        using var reader = new StreamReader(_contentBuffer, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Flush() => throw new NotSupportedException();
}
