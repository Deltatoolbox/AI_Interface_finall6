using Gateway.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DomainTokenValidationResult = Gateway.Domain.Interfaces.TokenValidationResult;

namespace Gateway.Infrastructure.Services;

public sealed class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    /// <summary>
    /// Generates a JWT token for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="username">Username</param>
    /// <param name="role">User role</param>
    /// <returns>JWT token string</returns>
    public string GenerateToken(Guid userId, string username, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Security:JwtKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Security:JwtIssuer"],
            audience: _configuration["Security:JwtAudience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return _tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token and extracts user information
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>Token validation result</returns>
    public DomainTokenValidationResult ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Security:JwtKey"]!));
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Security:JwtIssuer"],
                ValidAudience = _configuration["Security:JwtAudience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
            
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            var usernameClaim = principal.FindFirst(ClaimTypes.Name);
            var roleClaim = principal.FindFirst(ClaimTypes.Role);

            if (userIdClaim != null && usernameClaim != null && roleClaim != null &&
                Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return new DomainTokenValidationResult(true, userId, usernameClaim.Value, roleClaim.Value);
            }

            return new DomainTokenValidationResult(false, null, null, null);
        }
        catch
        {
            return new DomainTokenValidationResult(false, null, null, null);
        }
    }
}
