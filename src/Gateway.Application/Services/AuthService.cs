using Gateway.Application.DTOs;
using Gateway.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gateway.Application.Services;

public sealed class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user with username and password
    /// </summary>
    /// <param name="request">Login request containing username and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response with token or error</returns>
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
            
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Login attempt for non-existent or inactive user: {Username}", request.Username);
                return new LoginResponse(false, null, "Invalid credentials");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for user: {Username}", request.Username);
                return new LoginResponse(false, null, "Invalid credentials");
            }

            var token = _tokenService.GenerateToken(user.Id, user.Username, user.Role);
            
            _logger.LogInformation("User {Username} logged in successfully", request.Username);
            
            return new LoginResponse(true, token, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return new LoginResponse(false, null, "An error occurred during login");
        }
    }

    /// <summary>
    /// Validates a JWT token and returns user information
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>Token validation result</returns>
    public TokenValidationResult ValidateToken(string token)
    {
        return _tokenService.ValidateToken(token);
    }
}
