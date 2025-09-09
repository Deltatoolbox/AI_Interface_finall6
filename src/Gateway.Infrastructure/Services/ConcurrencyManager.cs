using Gateway.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Gateway.Infrastructure.Services;

public sealed class ConcurrencyManager : IConcurrencyManager
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _modelSemaphores = new();
    private readonly ConcurrentDictionary<Guid, int> _userStreamCounts = new();
    private readonly ILogger<ConcurrencyManager> _logger;
    private readonly int _maxConcurrentPerModel;
    private readonly int _maxActiveStreamsPerUser;

    public ConcurrencyManager(ILogger<ConcurrencyManager> logger, IConfiguration configuration)
    {
        _logger = logger;
        _maxConcurrentPerModel = configuration.GetValue<int>("Limits:MaxConcurrentPerModel", 2);
        _maxActiveStreamsPerUser = configuration.GetValue<int>("Limits:MaxActiveStreamsPerUser", 2);
    }

    /// <summary>
    /// Acquires a semaphore for the specified model
    /// </summary>
    /// <param name="model">Model name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Disposable semaphore holder</returns>
    public async Task<IDisposable> AcquireModelSemaphoreAsync(string model, CancellationToken cancellationToken = default)
    {
        var semaphore = _modelSemaphores.GetOrAdd(model, _ => new SemaphoreSlim(_maxConcurrentPerModel, _maxConcurrentPerModel));
        
        await semaphore.WaitAsync(cancellationToken);
        
        _logger.LogDebug("Acquired semaphore for model {Model}", model);
        
        return new SemaphoreRelease(semaphore, model, _logger);
    }

    /// <summary>
    /// Tries to acquire a stream slot for the specified user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if slot was acquired, false if limit reached</returns>
    public async Task<bool> TryAcquireUserStreamSlotAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var currentCount = _userStreamCounts.GetOrAdd(userId, 0);
        
        if (currentCount >= _maxActiveStreamsPerUser)
        {
            _logger.LogWarning("User {UserId} has reached maximum active streams limit ({Limit})", 
                userId, _maxActiveStreamsPerUser);
            return false;
        }

        var newCount = _userStreamCounts.AddOrUpdate(userId, 1, (_, existing) => existing + 1);
        
        _logger.LogDebug("User {UserId} acquired stream slot ({Current}/{Max})", 
            userId, newCount, _maxActiveStreamsPerUser);
        
        return true;
    }

    /// <summary>
    /// Releases a stream slot for the specified user
    /// </summary>
    /// <param name="userId">User ID</param>
    public void ReleaseUserStreamSlot(Guid userId)
    {
        var newCount = _userStreamCounts.AddOrUpdate(userId, 0, (_, existing) => Math.Max(0, existing - 1));
        
        if (newCount == 0)
        {
            _userStreamCounts.TryRemove(userId, out _);
        }
        
        _logger.LogDebug("User {UserId} released stream slot ({Current}/{Max})", 
            userId, newCount, _maxActiveStreamsPerUser);
    }

    private sealed class SemaphoreRelease : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly string _model;
        private readonly ILogger _logger;
        private bool _disposed = false;

        public SemaphoreRelease(SemaphoreSlim semaphore, string model, ILogger logger)
        {
            _semaphore = semaphore;
            _model = model;
            _logger = logger;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore.Release();
                _logger.LogDebug("Released semaphore for model {Model}", _model);
                _disposed = true;
            }
        }
    }
}
