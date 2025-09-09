using FluentAssertions;
using Gateway.Application.Validators;
using Gateway.Application.DTOs;
using Xunit;

namespace Gateway.UnitTests.Validators;

public class ChatRequestValidatorTests
{
    private readonly ChatRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_ShouldPass()
    {
        var request = new ChatRequest(
            "test-model",
            new[] { new ChatMessageDto("user", "Hello") },
            temperature: 0.7,
            maxTokens: 100
        );
        
        var result = _validator.Validate(request);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyMessages_ShouldFail()
    {
        var request = new ChatRequest(
            "test-model",
            Array.Empty<ChatMessageDto>()
        );
        
        var result = _validator.Validate(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalid-role")]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_InvalidRole_ShouldFail(string role)
    {
        var request = new ChatRequest(
            "test-model",
            new[] { new ChatMessageDto(role, "Hello") }
        );
        
        var result = _validator.Validate(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(2.1)]
    public void Validate_InvalidTemperature_ShouldFail(double temperature)
    {
        var request = new ChatRequest(
            "test-model",
            new[] { new ChatMessageDto("user", "Hello") },
            temperature: temperature
        );
        
        var result = _validator.Validate(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100001)]
    public void Validate_InvalidMaxTokens_ShouldFail(int maxTokens)
    {
        var request = new ChatRequest(
            "test-model",
            new[] { new ChatMessageDto("user", "Hello") },
            maxTokens: maxTokens
        );
        
        var result = _validator.Validate(request);
        
        result.IsValid.Should().BeFalse();
    }
}
