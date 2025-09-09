using Gateway.Domain.Entities;
using Gateway.Domain.Interfaces;
using Gateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Repositories;

public sealed class ConversationRepository : IConversationRepository
{
    private readonly GatewayDbContext _context;

    public ConversationRepository(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);
        return conversation;
    }

    public async Task<Conversation> UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        _context.Conversations.Update(conversation);
        await _context.SaveChangesAsync(cancellationToken);
        return conversation;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var conversation = await GetByIdAsync(id, cancellationToken);
        if (conversation != null)
        {
            _context.Conversations.Remove(conversation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
