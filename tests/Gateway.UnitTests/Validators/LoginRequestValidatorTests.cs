using FluentAssertions;
using Gateway.Application.Validators;
using Gateway.Application.DTOs;
using Xunit;

namespace Gateway.UnitTests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_ShouldPass()
    {
        var request = new LoginRequest("testuser", "password123");
        
        var result = _validator.Validate(request);
        
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData(null, "password")]
    [InlineData("user", "")]
    [InlineData("user", null)]
    public void Validate_InvalidRequest_ShouldFail(string username, string password)
    {
        var request = new LoginRequest(username, password);
        
        var result = _validator.Validate(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_UsernameTooLong_ShouldFail()
    {
        var request = new LoginRequest(new string('a', 51), "password");
        
        var result = _validator.Validate(request);
        
        result.IsValid.Should().BeFalse();
    }
}
