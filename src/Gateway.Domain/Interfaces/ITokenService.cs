namespace Gateway.Domain.Interfaces;

public interface ITokenService
{
    string GenerateToken(Guid userId, string username, string role);
    TokenValidationResult ValidateToken(string token);
}

public record TokenValidationResult(bool IsValid, Guid? UserId, string? Username, string? Role);
