using FluentAssertions;
using Gateway.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gateway.UnitTests.Services;

public class ConcurrencyManagerTests
{
    private readonly Mock<ILogger<ConcurrencyManager>> _loggerMock;
    private readonly ConcurrencyManager _concurrencyManager;

    public ConcurrencyManagerTests()
    {
        _loggerMock = new Mock<ILogger<ConcurrencyManager>>();
        
        var inMemorySettings = new Dictionary<string, string> {
            {"Limits:MaxConcurrentPerModel", "2"},
            {"Limits:MaxActiveStreamsPerUser", "2"},
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _concurrencyManager = new ConcurrencyManager(_loggerMock.Object, configuration);
    }

    [Fact]
    public async Task AcquireModelSemaphoreAsync_ShouldReturnDisposable()
    {
        using var semaphore = await _concurrencyManager.AcquireModelSemaphoreAsync("test-model");
        
        semaphore.Should().NotBeNull();
    }

    [Fact]
    public async Task TryAcquireUserStreamSlotAsync_ValidUser_ShouldReturnTrue()
    {
        var userId = Guid.NewGuid();
        
        var result = await _concurrencyManager.TryAcquireUserStreamSlotAsync(userId);
        
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryAcquireUserStreamSlotAsync_ExceedLimit_ShouldReturnFalse()
    {
        var userId = Guid.NewGuid();
        
        await _concurrencyManager.TryAcquireUserStreamSlotAsync(userId);
        await _concurrencyManager.TryAcquireUserStreamSlotAsync(userId);
        var result = await _concurrencyManager.TryAcquireUserStreamSlotAsync(userId);
        
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReleaseUserStreamSlot_ShouldDecreaseCount()
    {
        var userId = Guid.NewGuid();
        
        await _concurrencyManager.TryAcquireUserStreamSlotAsync(userId);
        await _concurrencyManager.TryAcquireUserStreamSlotAsync(userId);
        
        _concurrencyManager.ReleaseUserStreamSlot(userId);
        
        var result = await _concurrencyManager.TryAcquireUserStreamSlotAsync(userId);
        result.Should().BeTrue();
    }
}
