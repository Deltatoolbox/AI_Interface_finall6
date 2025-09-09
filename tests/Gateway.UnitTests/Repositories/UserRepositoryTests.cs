using FluentAssertions;
using Gateway.Infrastructure.Data;
using Gateway.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gateway.UnitTests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly GatewayDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<GatewayDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new GatewayDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_ValidUser_ShouldCreateUser()
    {
        var user = new Gateway.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = "hashedpassword",
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            DailyTokenQuota = 100000
        };

        var result = await _repository.CreateAsync(user);

        result.Should().NotBeNull();
        result.Username.Should().Be("testuser");
        
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        savedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_ExistingUser_ShouldReturnUser()
    {
        var user = new Gateway.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = "hashedpassword",
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            DailyTokenQuota = 100000
        };

        await _repository.CreateAsync(user);
        
        var result = await _repository.GetByUsernameAsync("testuser");
        
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByUsernameAsync_NonExistingUser_ShouldReturnNull()
    {
        var result = await _repository.GetByUsernameAsync("nonexistent");
        
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        var user1 = new Gateway.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = "user1",
            PasswordHash = "hash1",
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            DailyTokenQuota = 100000
        };

        var user2 = new Gateway.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = "user2",
            PasswordHash = "hash2",
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            DailyTokenQuota = 100000
        };

        await _repository.CreateAsync(user1);
        await _repository.CreateAsync(user2);
        
        var result = await _repository.GetAllAsync();
        
        result.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
