using Gateway.Domain.Entities;
using Gateway.Domain.Interfaces;
using Gateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Repositories;

public sealed class MessageRepository : IMessageRepository
{
    private readonly GatewayDbContext _context;

    public MessageRepository(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Include(m => m.Conversation)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Message>> GetByConversationIdAsync(
        Guid conversationId, 
        int page = 1, 
        int pageSize = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task<Message> UpdateAsync(Message message, CancellationToken cancellationToken = default)
    {
        _context.Messages.Update(message);
        await _context.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await GetByIdAsync(id, cancellationToken);
        if (message != null)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
