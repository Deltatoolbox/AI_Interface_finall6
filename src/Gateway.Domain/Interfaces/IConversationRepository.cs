using Gateway.Domain.Entities;

namespace Gateway.Domain.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task<Conversation> UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
