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
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ConcurrencyManager _concurrencyManager;

    public ConcurrencyManagerTests()
    {
        _loggerMock = new Mock<ILogger<ConcurrencyManager>>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x.GetValue<int>("Limits:MaxConcurrentPerModel", 2)).Returns(2);
        _configurationMock.Setup(x => x.GetValue<int>("Limits:MaxActiveStreamsPerUser", 2)).Returns(2);
        
        _concurrencyManager = new ConcurrencyManager(_loggerMock.Object, _configurationMock.Object);
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
    public void ReleaseUserStreamSlot_ShouldDecreaseCount()
    {
        var userId = Guid.NewGuid();
        
        _concurrencyManager.TryAcquireUserStreamSlotAsync(userId).Wait();
        _concurrencyManager.TryAcquireUserStreamSlotAsync(userId).Wait();
        
        _concurrencyManager.ReleaseUserStreamSlot(userId);
        
        var result = _concurrencyManager.TryAcquireUserStreamSlotAsync(userId).Result;
        result.Should().BeTrue();
    }
}
