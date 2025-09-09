using Gateway.Domain.Entities;

namespace Gateway.Domain.Interfaces;

public interface IUsageLogRepository
{
    Task<UsageLog> CreateAsync(UsageLog usageLog, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageLog>> GetByUserIdAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<int> GetTotalTokensUsedByUserAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default);
}
