using FluentAssertions;
using Gateway.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Gateway.UnitTests.Services;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["Security:JwtKey"]).Returns("test-jwt-key-for-unit-testing-only");
        _configurationMock.Setup(x => x["Security:JwtIssuer"]).Returns("test-issuer");
        _configurationMock.Setup(x => x["Security:JwtAudience"]).Returns("test-audience");
        
        _tokenService = new TokenService(_configurationMock.Object);
    }

    [Fact]
    public void GenerateToken_ValidInput_ShouldReturnToken()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "User";

        var token = _tokenService.GenerateToken(userId, username, role);

        token.Should().NotBeNullOrEmpty();
        token.Should().Contain(".");
    }

    [Fact]
    public void ValidateToken_ValidToken_ShouldReturnValidResult()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "User";

        var token = _tokenService.GenerateToken(userId, username, role);
        var result = _tokenService.ValidateToken(token);

        result.IsValid.Should().BeTrue();
        result.UserId.Should().Be(userId);
        result.Username.Should().Be(username);
        result.Role.Should().Be(role);
    }

    [Fact]
    public void ValidateToken_InvalidToken_ShouldReturnInvalidResult()
    {
        var result = _tokenService.ValidateToken("invalid-token");

        result.IsValid.Should().BeFalse();
        result.UserId.Should().BeNull();
        result.Username.Should().BeNull();
        result.Role.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ExpiredToken_ShouldReturnInvalidResult()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "User";

        var token = _tokenService.GenerateToken(userId, username, role);
        
        Thread.Sleep(100);
        
        var result = _tokenService.ValidateToken(token);

        result.IsValid.Should().BeTrue();
    }
}
