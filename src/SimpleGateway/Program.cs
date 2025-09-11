using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using SimpleGateway.Data;
using SimpleGateway.Services;
using SimpleGateway.DTOs;
using SimpleGateway.Models;
using SimpleGateway.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<UserManagementSettings>(builder.Configuration.GetSection("UserManagement"));

// Add Entity Framework with SQLite
builder.Services.AddDbContext<GatewayDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=gateway.db"));

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add custom services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IShareService, ShareService>();
builder.Services.AddScoped<IJwtTokenService>(provider =>
{
    var jwtSettings = provider.GetRequiredService<IOptions<JwtSettings>>().Value;
    return new JwtTokenService(jwtSettings);
});

// CORS für Frontend-Integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
    context.Database.EnsureCreated();
    
    // Create default admin user if not exists
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    var adminUser = await userService.GetUserByUsernameAsync("admin");
    if (adminUser == null)
    {
        await userService.CreateUserAsync("admin", "admin", "", "Admin");
        Console.WriteLine("Default admin user created (admin/admin)");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS aktivieren
app.UseCors("AllowFrontend");

app.MapGet("/", () => "LM Gateway API is running!");

app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

app.MapGet("/api/models", async () =>
{
    try
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        
        var response = await httpClient.GetAsync("http://localhost:1234/v1/models");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"LM Studio Models: {content}");
            return Results.Content(content, "application/json");
        }
        
        Console.WriteLine("LM Studio nicht verfügbar, verwende Fallback-Modelle");
        
        return Results.Ok(new { 
            data = new[] { 
                new { id = "llama-2-7b", @object = "model", created = 1234567890, owned_by = "meta" },
                new { id = "gpt-3.5-turbo", @object = "model", created = 1234567890, owned_by = "openai" }
            }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fehler beim Abrufen der Modelle: {ex.Message}");
        
        return Results.Ok(new { 
            data = new[] { 
                new { id = "llama-2-7b", @object = "model", created = 1234567890, owned_by = "meta" },
                new { id = "gpt-3.5-turbo", @object = "model", created = 1234567890, owned_by = "openai" }
            }
        });
    }
});

app.MapGet("/api/auth/csrf", () => new { token = Guid.NewGuid().ToString() });

// Helper function to get current user from JWT token
async Task<User?> GetCurrentUserAsync(HttpContext context, IUserService userService, IJwtTokenService jwtService)
{
    // Try to get token from Authorization header first
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    string? token = null;
    
    if (authHeader?.StartsWith("Bearer ") == true)
    {
        token = authHeader.Substring("Bearer ".Length).Trim();
    }
    else
    {
        // Fallback to cookie
        token = context.Request.Cookies["access_token"];
    }
    
    if (string.IsNullOrEmpty(token))
        return null;

    // Validate token and get user ID
    var principal = jwtService.ValidateToken(token);
    if (principal == null)
        return null;

    var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return null;

    return await userService.GetUserByIdAsync(userId);
}

app.MapPost("/api/auth/login", async (LoginRequest request, HttpContext context, IUserService userService, IJwtTokenService jwtService) =>
{
    var user = await userService.GetUserByUsernameAsync(request.Username);
    
    if (user != null && await userService.ValidatePasswordAsync(user, request.Password))
    {
        var token = jwtService.GenerateToken(user);
        
        // Setze JWT Token als Cookie
        context.Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = false,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddHours(24),
            Domain = "localhost"
        });
        
        var userResponse = new UserResponse(user.Id, user.Username, user.Email, user.Role, user.CreatedAt);
        return Results.Ok(new LoginResponse(true, token, "Login successful", userResponse));
    }
    
    return Results.Unauthorized();
});

app.MapPost("/api/auth/logout", (HttpContext context) =>
{
    context.Response.Cookies.Delete("access_token");
    return Results.NoContent();
});

// User Registration API
app.MapPost("/api/auth/register", async (RegisterRequest request, IUserService userService, IConfiguration config) =>
{
    var userManagementSettings = config.GetSection("UserManagement").Get<UserManagementSettings>();
    
    if (!userManagementSettings?.AllowSelfRegistration ?? true)
    {
        return Results.BadRequest(new RegisterResponse(false, "Self-registration is disabled"));
    }

    if (await userService.UserExistsAsync(request.Username))
    {
        return Results.BadRequest(new RegisterResponse(false, "Username already exists"));
    }

    var user = await userService.CreateUserAsync(request.Username, request.Password, request.Email, userManagementSettings?.DefaultRole ?? "User");
    if (user == null)
    {
        return Results.BadRequest(new RegisterResponse(false, "Failed to create user"));
    }

    var userResponse = new UserResponse(user.Id, user.Username, user.Email, user.Role, user.CreatedAt);
    return Results.Ok(new RegisterResponse(true, "User created successfully", userResponse));
});

// Conversation endpoints
app.MapGet("/api/conversations", async (HttpContext context, IUserService userService, IConversationService conversationService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();
    
    var conversations = await conversationService.GetConversationsByUserIdAsync(user.Id);
    return Results.Ok(new { data = conversations });
});

app.MapPost("/api/conversations", async (CreateConversationRequest request, HttpContext context, IUserService userService, IConversationService conversationService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();
    
    var conversation = await conversationService.CreateConversationAsync(user.Id, request.Title, request.Model, request.Category);
    Console.WriteLine($"Conversation erstellt: {conversation.Id} - {request.Title}");
    
    return Results.Ok(conversation);
});

app.MapGet("/api/conversations/{id}", async (string id, HttpContext context, IUserService userService, IConversationService conversationService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();
    
    var conversation = await conversationService.GetConversationWithMessagesAsync(id, user.Id);
    if (conversation == null)
        return Results.NotFound();
    
    return Results.Ok(new { messages = conversation.Messages });
});

app.MapPut("/api/conversations/{id}", async (string id, UpdateConversationRequest request, HttpContext context, IUserService userService, IConversationService conversationService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();
    
    var conversation = await conversationService.UpdateConversationTitleAsync(id, user.Id, request.Title);
    if (conversation == null)
        return Results.NotFound();
    
    Console.WriteLine($"Conversation umbenannt: {id} - {request.Title}");
    
    return Results.Ok(conversation);
});

app.MapPost("/api/chat", async (ChatRequest request, HttpContext context, IUserService userService, IMessageService messageService, IConversationService conversationService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();
    
    try
    {
        using var httpClient = new HttpClient();
        
        // Bestimme das zu verwendende Model
        string modelToUse = request.Model;
        if (!string.IsNullOrEmpty(request.ConversationId))
        {
            var conversation = await conversationService.GetConversationWithMessagesAsync(request.ConversationId, user.Id);
            if (conversation != null && !string.IsNullOrEmpty(conversation.Model))
            {
                modelToUse = conversation.Model;
            }
        }
        
        var lmStudioRequest = new
        {
            model = modelToUse,
            messages = request.Messages.Select(m => new { role = m.Role, content = m.Content }),
            max_tokens = 1000,
            temperature = 0.7
        };
        
        var json = System.Text.Json.JsonSerializer.Serialize(lmStudioRequest);
        Console.WriteLine($"Sending to LM Studio: {json}");
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await httpClient.PostAsync("http://localhost:1234/v1/chat/completions", content);
        
        Console.WriteLine($"LM Studio Response Status: {response.StatusCode}");
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"LM Studio Response: {responseContent}");
            
            // Speichere Messages wenn ConversationId vorhanden ist
            if (!string.IsNullOrEmpty(request.ConversationId))
            {
                await messageService.SaveMessagesAsync(request.ConversationId, request.Messages);
                
                // Speichere Assistant Response
                var assistantContent = "Keine Antwort";
                try
                {
                    var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent);
                    if (jsonDoc.RootElement.TryGetProperty("choices", out var choices) && 
                        choices.GetArrayLength() > 0 &&
                        choices[0].TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var contentProp))
                    {
                        assistantContent = contentProp.GetString() ?? "Keine Antwort";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Parsen der LM Studio Antwort: {ex.Message}");
                }
                
                await messageService.SaveAssistantMessageAsync(request.ConversationId, assistantContent);
                Console.WriteLine($"Messages gespeichert für Conversation: {request.ConversationId}");
            }
            
            return Results.Content(responseContent, "application/json");
        }
        
        // Fallback zu Mock-Antwort wenn LM Studio nicht verfügbar
        return Results.Ok(new { 
            id = "chatcmpl-123",
            @object = "chat.completion",
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            model = request.Model,
            choices = new[] {
                new {
                    index = 0,
                    message = new {
                        role = "assistant",
                        content = $"Echo: {request.Messages.LastOrDefault()?.Content ?? "Hello!"}"
                    },
                    finish_reason = "stop"
                }
            },
            usage = new {
                prompt_tokens = 10,
                completion_tokens = 5,
                total_tokens = 15
            }
        });
    }
    catch
    {
        // Fallback zu Mock-Antwort bei Fehlern
        return Results.Ok(new { 
            id = "chatcmpl-123",
            @object = "chat.completion",
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            model = request.Model,
            choices = new[] {
                new {
                    index = 0,
                    message = new {
                        role = "assistant",
                        content = $"Echo: {request.Messages.LastOrDefault()?.Content ?? "Hello!"}"
                    },
                    finish_reason = "stop"
                }
            },
            usage = new {
                prompt_tokens = 10,
                completion_tokens = 5,
                total_tokens = 15
            }
        });
    }
});

// Admin User Management APIs
app.MapGet("/api/admin/users", async (HttpContext context, IUserService userService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null || user.Role != "Admin")
        return Results.Unauthorized();

    var users = await userService.GetAllUsersAsync();
    var userResponses = users.Select(u => new UserResponse(u.Id, u.Username, u.Email, u.Role, u.CreatedAt)).ToList();
    return Results.Ok(userResponses);
});

app.MapPost("/api/admin/users", async (CreateUserRequest request, HttpContext context, IUserService userService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null || user.Role != "Admin")
        return Results.Unauthorized();

    if (await userService.UserExistsAsync(request.Username))
    {
        return Results.BadRequest(new { message = "Username already exists" });
    }

    var newUser = await userService.CreateUserAsync(request.Username, request.Password, request.Email, request.Role);
    if (newUser == null)
    {
        return Results.BadRequest(new { message = "Failed to create user" });
    }

    var userResponse = new UserResponse(newUser.Id, newUser.Username, newUser.Email, newUser.Role, newUser.CreatedAt);
    return Results.Ok(userResponse);
});

app.MapPut("/api/admin/users/{id}", async (string id, UpdateUserRequest request, HttpContext context, IUserService userService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null || user.Role != "Admin")
        return Results.Unauthorized();

    var updatedUser = await userService.UpdateUserAsync(id, request.Username, request.Email, request.Role);
    if (updatedUser == null)
        return Results.NotFound();

    var userResponse = new UserResponse(updatedUser.Id, updatedUser.Username, updatedUser.Email, updatedUser.Role, updatedUser.CreatedAt);
    return Results.Ok(userResponse);
});

app.MapDelete("/api/admin/users/{id}", async (string id, HttpContext context, IUserService userService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null || user.Role != "Admin")
        return Results.Unauthorized();

    if (user.Id == id)
        return Results.BadRequest(new { message = "Cannot delete your own account" });

    var success = await userService.DeleteUserAsync(id);
    if (!success)
        return Results.NotFound();

    return Results.NoContent();
});

        // Admin stats endpoint
        app.MapGet("/api/admin/stats", async (HttpContext context, IUserService userService, IConversationService conversationService, IJwtTokenService jwtService, GatewayDbContext dbContext) =>
        {
            var user = await GetCurrentUserAsync(context, userService, jwtService);
            if (user == null || user.Role != "Admin")
                return Results.Unauthorized();

            try
            {
                // Get all users
                var users = await userService.GetAllUsersAsync();
                var totalUsers = users.Count;

                // Get all conversations from all users
                var allConversations = new List<ConversationResponse>();
                var totalMessages = 0;
                
                foreach (var u in users)
                {
                    var userConversations = await conversationService.GetConversationsByUserIdAsync(u.Id);
                    allConversations.AddRange(userConversations);
                    
                    // Count messages for each conversation using direct DbContext
                    foreach (var conv in userConversations)
                    {
                        var messageCount = await dbContext.Messages
                            .Where(m => m.ConversationId == conv.Id)
                            .CountAsync();
                        totalMessages += messageCount;
                    }
                }

                var stats = new
                {
                    totalUsers,
                    totalConversations = allConversations.Count,
                    totalMessages,
                    lastUpdated = DateTime.UtcNow
                };

                return Results.Ok(stats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting admin stats: {ex.Message}");
                return Results.Problem("Failed to get admin statistics");
            }
        });

// Privacy & Security API Endpoints
app.MapDelete("/api/conversations", async (HttpContext context, IConversationService conversationService, IUserService userService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    var success = await conversationService.DeleteAllConversationsForUserAsync(user.Id);
    if (!success)
        return Results.BadRequest(new { message = "Failed to delete conversations" });

    return Results.Ok(new { message = "All conversations deleted successfully" });
}).AllowAnonymous();

app.MapPost("/api/auth/change-password", async (ChangePasswordRequest request, HttpContext context, IUserService userService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    // Verify current password
    var isValidPassword = await userService.ValidatePasswordAsync(user.Id, request.CurrentPassword);
    if (!isValidPassword)
        return Results.BadRequest(new { message = "Current password is incorrect" });

    // Update password
    var success = await userService.UpdatePasswordAsync(user.Id, request.NewPassword);
    if (!success)
        return Results.BadRequest(new { message = "Failed to update password" });

    return Results.Ok(new { message = "Password updated successfully" });
}).AllowAnonymous();

// Admin password reset endpoint
app.MapPost("/api/admin/reset-password", async (ResetPasswordRequest request, HttpContext context, IUserService userService, IJwtTokenService jwtService) =>
{
    var adminUser = await GetCurrentUserAsync(context, userService, jwtService);
    if (adminUser == null || adminUser.Role != "Admin")
        return Results.Unauthorized();

    // Find the user to reset password for
    var targetUser = await userService.GetUserByUsernameAsync(request.Username);
    if (targetUser == null)
        return Results.BadRequest(new { message = "User not found" });

    // Update password
    var success = await userService.UpdatePasswordAsync(targetUser.Id, request.NewPassword);
    if (!success)
        return Results.BadRequest(new { message = "Failed to reset password" });

    return Results.Ok(new { message = $"Password reset successfully for user {request.Username}" });
});

// Export/Import API Endpoints
app.MapGet("/api/conversations/export", async (HttpContext context, IConversationService conversationService, IUserService userService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    try
    {
        var conversations = await conversationService.GetAllConversationsForUserAsync(user.Id);
        var exportData = new List<ConversationExportData>();

        foreach (var conv in conversations)
        {
            var messages = await conversationService.GetMessagesByConversationIdAsync(conv.Id);
            var messageExports = messages.Select(m => new MessageExportData(m.Role, m.Content, m.CreatedAt)).ToArray();
            
            exportData.Add(new ConversationExportData(
                conv.Id,
                conv.Title,
                conv.CreatedAt,
                conv.UpdatedAt,
                messageExports
            ));
        }

        return Results.Ok(exportData);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error exporting conversations: {ex.Message}");
        return Results.Problem("Failed to export conversations");
    }
});

app.MapPost("/api/conversations/import", async (ConversationImportRequest request, HttpContext context, IConversationService conversationService, IUserService userService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    try
    {
        var importedCount = 0;
        var errors = new List<string>();

        foreach (var convData in request.Conversations)
        {
            try
            {
                // Create new conversation with new ID
                var newConversationId = Guid.NewGuid().ToString();
                var conversation = new Conversation
                {
                    Id = newConversationId,
                    Title = convData.Title,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await conversationService.CreateConversationAsync(conversation);

                // Add messages
                foreach (var msgData in convData.Messages)
                {
                    var message = new Message
                    {
                        Id = Guid.NewGuid().ToString(),
                        ConversationId = newConversationId,
                        Role = msgData.Role,
                        Content = msgData.Content,
                        CreatedAt = DateTime.UtcNow
                    };

                    await conversationService.AddMessageAsync(message);
                }

                importedCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to import conversation '{convData.Title}': {ex.Message}");
            }
        }

        var result = new
        {
            importedCount,
            totalCount = request.Conversations.Length,
            errors = errors.ToArray()
        };

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error importing conversations: {ex.Message}");
        return Results.Problem("Failed to import conversations");
    }
});

// Search API Endpoint
app.MapPost("/api/search", async (SearchRequest request, HttpContext context, IConversationService conversationService, IUserService userService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    try
    {
        var results = await conversationService.SearchMessagesAsync(user.Id, request.Query, request.Limit, request.Offset);
        return Results.Ok(new { results, totalCount = results.Length });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error searching messages: {ex.Message}");
        return Results.Problem("Failed to search messages");
    }
});

// Chat Sharing Endpoints
app.MapPost("/api/shares", async (CreateShareRequest request, HttpContext context, IUserService userService, IShareService shareService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    try
    {
        var share = await shareService.CreateShareAsync(user.Id, request.ConversationId, request.Password, request.ExpiresAt);
        return Results.Ok(share);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating share: {ex.Message}");
        return Results.Problem("Failed to create share");
    }
});

app.MapGet("/api/shares/{shareId}", async (string shareId, string? password, IShareService shareService) =>
{
    try
    {
        var conversation = await shareService.GetSharedConversationAsync(shareId, password);
        if (conversation == null)
            return Results.NotFound("Share not found or expired");

        return Results.Ok(conversation);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error accessing shared conversation: {ex.Message}");
        return Results.Problem("Failed to access shared conversation");
    }
});

app.MapDelete("/api/shares/{shareId}", async (string shareId, HttpContext context, IUserService userService, IShareService shareService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    try
    {
        var success = await shareService.RevokeShareAsync(user.Id, shareId);
        if (!success)
            return Results.NotFound("Share not found");

        return Results.Ok(new { message = "Share revoked successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error revoking share: {ex.Message}");
        return Results.Problem("Failed to revoke share");
    }
});

app.MapGet("/api/shares", async (HttpContext context, IUserService userService, IShareService shareService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    try
    {
        var shares = await shareService.GetUserSharesAsync(user.Id);
        return Results.Ok(shares);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting user shares: {ex.Message}");
        return Results.Problem("Failed to get user shares");
    }
});

app.Run();