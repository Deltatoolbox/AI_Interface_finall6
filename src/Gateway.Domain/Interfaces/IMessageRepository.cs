using Gateway.Domain.Entities;

namespace Gateway.Domain.Interfaces;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Message>> GetByConversationIdAsync(Guid conversationId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default);
    Task<Message> UpdateAsync(Message message, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
