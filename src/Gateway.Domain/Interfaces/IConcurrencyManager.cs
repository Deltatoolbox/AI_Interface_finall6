namespace Gateway.Domain.Interfaces;

public interface IConcurrencyManager
{
    Task<IDisposable> AcquireModelSemaphoreAsync(string model, CancellationToken cancellationToken = default);
    Task<bool> TryAcquireUserStreamSlotAsync(Guid userId, CancellationToken cancellationToken = default);
    void ReleaseUserStreamSlot(Guid userId);
}
