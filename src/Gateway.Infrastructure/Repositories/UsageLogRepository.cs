using Gateway.Domain.Entities;
using Gateway.Domain.Interfaces;
using Gateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Repositories;

public sealed class UsageLogRepository : IUsageLogRepository
{
    private readonly GatewayDbContext _context;

    public UsageLogRepository(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<UsageLog> CreateAsync(UsageLog usageLog, CancellationToken cancellationToken = default)
    {
        _context.UsageLogs.Add(usageLog);
        await _context.SaveChangesAsync(cancellationToken);
        return usageLog;
    }

    public async Task<IEnumerable<UsageLog>> GetByUserIdAsync(
        Guid userId, 
        DateTime? from = null, 
        DateTime? to = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.UsageLogs.Where(u => u.UserId == userId);

        if (from.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= to.Value);
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalTokensUsedByUserAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.UsageLogs
            .Where(u => u.UserId == userId && u.CreatedAt >= startOfDay && u.CreatedAt < endOfDay)
            .SumAsync(u => u.TotalTokens, cancellationToken);
    }
}
