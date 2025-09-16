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
using SimpleGateway.Plugins;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "LM Gateway API",
        Version = "v1",
        Description = "API für das LM Gateway System mit Chat, User Management, Webhooks und mehr"
    });
    
    // JWT Authentication für Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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
builder.Services.AddScoped<IChatTemplateService, ChatTemplateService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IHealthMonitoringService, HealthMonitoringService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<ISsoService, SsoService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IGdprService, GdprService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddScoped<IPluginManager, PluginManager>();
builder.Services.AddScoped<ISlackService, SlackService>();
builder.Services.AddScoped<IDiscordService, DiscordService>();
builder.Services.AddHttpClient();
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

// Swagger UI für API-Dokumentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LM Gateway API v1");
        c.RoutePrefix = "api-docs"; // Swagger UI unter /api-docs verfügbar
    });
}
app.UseAuthentication();
app.UseAuthorization();

// Ensure database is created
using (var dbScope = app.Services.CreateScope())
{
    var context = dbScope.ServiceProvider.GetRequiredService<GatewayDbContext>();
    context.Database.EnsureCreated();
    
    // Create default admin user if not exists
    var userService = dbScope.ServiceProvider.GetRequiredService<IUserService>();
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

// Model Management API Endpoints (Admin only)
app.MapGet("/api/models/status", async () =>
{
    try
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        
        var response = await httpClient.GetAsync("http://localhost:1234/v1/models");
        
        return Results.Ok(new { 
            connected = response.IsSuccessStatusCode,
            lastCheck = DateTime.UtcNow,
            errorMessage = response.IsSuccessStatusCode ? null : "LM Studio not available"
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { 
            connected = false,
            lastCheck = DateTime.UtcNow,
            errorMessage = ex.Message
        });
    }
});

app.MapPost("/api/admin/models/set-default", async (SetDefaultModelRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.models.manage"))
        return Results.Forbid();
    
    try
    {
        // Store the default model in configuration or database
        // For now, we'll just validate the model exists
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        
        var response = await httpClient.GetAsync("http://localhost:1234/v1/models");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var modelsData = System.Text.Json.JsonDocument.Parse(content);
            var models = modelsData.RootElement.GetProperty("data").EnumerateArray();
            
            var modelExists = models.Any(m => m.GetProperty("id").GetString() == request.ModelId);
            
            if (!modelExists)
            {
                return Results.BadRequest(new { success = false, message = "Model not found in LM Studio" });
            }
        }
        
        // TODO: Store default model in database or configuration
        Console.WriteLine($"Default model set to: {request.ModelId}");
        
        return Results.Ok(new { success = true, message = "Default model updated successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error setting default model: {ex.Message}");
        return Results.Problem("Failed to set default model");
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
app.MapPost("/api/auth/register", async (RegisterRequest request, IUserService userService, IConfiguration config, IWebhookService webhookService) =>
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

    // Trigger webhook event
    await webhookService.TriggerWebhookAsync("user.created", new {
        userId = user.Id,
        username = user.Username,
        email = user.Email,
        createdAt = user.CreatedAt
    });

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

app.MapPost("/api/conversations", async (CreateConversationRequest request, HttpContext context, IUserService userService, IConversationService conversationService, IJwtTokenService jwtService, IAuditService auditService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();
    
    var conversation = await conversationService.CreateConversationAsync(user.Id, request.Title, request.Model, request.Category);
    Console.WriteLine($"Conversation erstellt: {conversation.Id} - {request.Title}");
    
    // Log the conversation creation
    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var userAgent = context.Request.Headers.UserAgent.ToString();
    
    await auditService.LogActionAsync(
        user.Id, 
        user.Username, 
        "conversation.created", 
        "conversation", 
        $"Created conversation: {request.Title}", 
        ipAddress, 
        userAgent
    );
    
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

app.MapPost("/api/chat", async (ChatRequest request, HttpContext context, IUserService userService, IMessageService messageService, IConversationService conversationService, IJwtTokenService jwtService, IAuditService auditService, IEncryptionService encryptionService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();
    
    // Log the chat action
    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var userAgent = context.Request.Headers.UserAgent.ToString();
    var lastMessage = request.Messages.LastOrDefault();
    var messageContent = lastMessage?.Content?.Substring(0, Math.Min(100, lastMessage.Content.Length)) ?? "empty";
    
    await auditService.LogActionAsync(
        user.Id, 
        user.Username, 
        "chat.message_sent", 
        "conversation", 
        $"Sent message: {messageContent}...", 
        ipAddress, 
        userAgent
    );
    
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
                // Encrypt messages before saving
                var encryptedMessages = new List<MessageDto>();
                foreach (var msg in request.Messages)
                {
                    var encryptedContent = await encryptionService.EncryptMessageAsync(msg.Content, user.Id);
                    encryptedMessages.Add(new MessageDto(msg.Role, encryptedContent));
                }
                
                await messageService.SaveMessagesAsync(request.ConversationId, encryptedMessages.ToArray());
                
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
                
                // Encrypt assistant response before saving
                var encryptedAssistantContent = await encryptionService.EncryptMessageAsync(assistantContent, user.Id);
                await messageService.SaveAssistantMessageAsync(request.ConversationId, encryptedAssistantContent);
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

app.MapPost("/api/admin/users", async (CreateUserRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "users.create"))
        return Results.Forbid();

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

app.MapPut("/api/admin/users/{id}", async (string id, UpdateUserRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "users.edit"))
        return Results.Forbid();

    var updatedUser = await userService.UpdateUserAsync(id, request.Username, request.Email, request.Role);
    if (updatedUser == null)
        return Results.NotFound();

    var userResponse = new UserResponse(updatedUser.Id, updatedUser.Username, updatedUser.Email, updatedUser.Role, updatedUser.CreatedAt);
    return Results.Ok(userResponse);
});

app.MapDelete("/api/admin/users/{id}", async (string id, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "users.delete"))
        return Results.Forbid();

    if (user.Id == id)
        return Results.BadRequest(new { message = "Cannot delete your own account" });

    var success = await userService.DeleteUserAsync(id);
    if (!success)
        return Results.NotFound();

    return Results.NoContent();
});

        // Admin stats endpoint
        app.MapGet("/api/admin/stats", async (HttpContext context, IUserService userService, IUserRoleService roleService, IConversationService conversationService, IJwtTokenService jwtService, GatewayDbContext dbContext) =>
        {
            var user = await GetCurrentUserAsync(context, userService, jwtService);
            if (user == null)
                return Results.Unauthorized();

            if (!await roleService.UserHasPermissionAsync(user.Id, "admin.dashboard"))
                return Results.Forbid();

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

app.MapDelete("/api/conversations/{conversationId}", async (string conversationId, HttpContext context, IConversationService conversationService, IUserService userService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    var success = await conversationService.DeleteConversationAsync(conversationId, user.Id);
    if (!success)
        return Results.BadRequest(new { message = "Failed to delete conversation" });

    return Results.Ok(new { message = "Conversation deleted successfully" });
});

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

// Chat Templates Endpoints
app.MapGet("/api/templates", async (HttpContext context, IUserService userService, IUserRoleService roleService, IChatTemplateService templateService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "templates.view"))
        return Results.Forbid();

    try
    {
        var templates = await templateService.GetAllTemplatesAsync();
        return Results.Ok(templates);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting templates: {ex.Message}");
        return Results.Problem("Failed to get templates");
    }
});

app.MapGet("/api/templates/categories", async (HttpContext context, IUserService userService, IUserRoleService roleService, IChatTemplateService templateService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "templates.view"))
        return Results.Forbid();

    try
    {
        var categories = await templateService.GetCategoriesAsync();
        return Results.Ok(categories);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting categories: {ex.Message}");
        return Results.Problem("Failed to get categories");
    }
});

app.MapGet("/api/templates/{templateId}", async (string templateId, HttpContext context, IUserService userService, IUserRoleService roleService, IChatTemplateService templateService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "templates.view"))
        return Results.Forbid();

    try
    {
        var template = await templateService.GetTemplateByIdAsync(templateId);
        if (template == null)
            return Results.NotFound("Template not found");

        return Results.Ok(template);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting template: {ex.Message}");
        return Results.Problem("Failed to get template");
    }
});

app.MapGet("/api/templates/category/{category}", async (string category, HttpContext context, IUserService userService, IUserRoleService roleService, IChatTemplateService templateService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "templates.view"))
        return Results.Forbid();

    try
    {
        var templates = await templateService.GetTemplatesByCategoryAsync(category);
        return Results.Ok(templates);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting templates by category: {ex.Message}");
        return Results.Problem("Failed to get templates by category");
    }
});

app.MapPost("/api/templates", async (CreateTemplateRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IChatTemplateService templateService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "templates.create"))
        return Results.Forbid();

    try
    {
        var template = await templateService.CreateTemplateAsync(user.Id, request);
        return Results.Created($"/api/templates/{template.Id}", template);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating template: {ex.Message}");
        return Results.Problem("Failed to create template");
    }
});

app.MapPut("/api/templates/{templateId}", async (string templateId, CreateTemplateRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IChatTemplateService templateService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "templates.edit"))
        return Results.Forbid();

    try
    {
        var success = await templateService.UpdateTemplateAsync(user.Id, templateId, request);
        if (!success)
            return Results.NotFound("Template not found or access denied");

        return Results.Ok(new { message = "Template updated successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating template: {ex.Message}");
        return Results.Problem("Failed to update template");
    }
});

app.MapDelete("/api/templates/{templateId}", async (string templateId, HttpContext context, IUserService userService, IUserRoleService roleService, IChatTemplateService templateService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "templates.delete"))
        return Results.Forbid();

    try
    {
        var success = await templateService.DeleteTemplateAsync(user.Id, templateId);
        if (!success)
            return Results.NotFound("Template not found or access denied");

        return Results.Ok(new { message = "Template deleted successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting template: {ex.Message}");
        return Results.Problem("Failed to delete template");
    }
});

app.MapPost("/api/templates/seed", async (HttpContext context, IUserService userService, IUserRoleService roleService, IChatTemplateService templateService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "templates.create"))
        return Results.Forbid();

    try
    {
        await templateService.SeedBuiltInTemplatesAsync();
        return Results.Ok(new { message = "Built-in templates seeded successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding templates: {ex.Message}");
        return Results.Problem("Failed to seed templates");
    }
});

// Admin Backup/Restore Endpoints
app.MapGet("/api/admin/backups", async (HttpContext context, IUserService userService, IUserRoleService roleService, IBackupService backupService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.backups"))
        return Results.Forbid();

    try
    {
        var backups = await backupService.GetBackupsAsync();
        return Results.Ok(backups);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting backups: {ex.Message}");
        return Results.Problem("Failed to get backups");
    }
});

app.MapPost("/api/admin/backups", async (CreateBackupRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IBackupService backupService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.backups"))
        return Results.Forbid();

    try
    {
        var backup = await backupService.CreateBackupAsync(request.Name, request.Description);
        return Results.Created($"/api/admin/backups/{backup.Id}", backup);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating backup: {ex.Message}");
        return Results.Problem("Failed to create backup");
    }
});

app.MapPost("/api/admin/backups/{backupId}/restore", async (string backupId, HttpContext context, IUserService userService, IUserRoleService roleService, IBackupService backupService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.backups"))
        return Results.Forbid();

    try
    {
        var success = await backupService.RestoreBackupAsync(backupId);
        if (!success)
            return Results.NotFound("Backup not found");

        return Results.Ok(new { message = "Backup restored successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error restoring backup: {ex.Message}");
        return Results.Problem("Failed to restore backup");
    }
});

app.MapDelete("/api/admin/backups/{backupId}", async (string backupId, HttpContext context, IUserService userService, IUserRoleService roleService, IBackupService backupService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.backups"))
        return Results.Forbid();

    try
    {
        var success = await backupService.DeleteBackupAsync(backupId);
        if (!success)
            return Results.NotFound("Backup not found");

        return Results.Ok(new { message = "Backup deleted successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting backup: {ex.Message}");
        return Results.Problem("Failed to delete backup");
    }
});

app.MapGet("/api/admin/backups/{backupId}/download", async (string backupId, HttpContext context, IUserService userService, IUserRoleService roleService, IBackupService backupService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.backups"))
        return Results.Forbid();

    try
    {
        var backupData = await backupService.DownloadBackupAsync(backupId);
        return Results.File(backupData, "application/octet-stream", $"{backupId}.db");
    }
    catch (FileNotFoundException)
    {
        return Results.NotFound("Backup file not found");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error downloading backup: {ex.Message}");
        return Results.Problem("Failed to download backup");
    }
});

app.MapPost("/api/admin/backups/upload", async (HttpContext context, IUserService userService, IUserRoleService roleService, IBackupService backupService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.backups"))
        return Results.Forbid();

    try
    {
        var form = await context.Request.ReadFormAsync();
        var file = form.Files["backup"];
        var name = form["name"].ToString();
        var description = form["description"].ToString();

        // Validate input
        if (file == null || string.IsNullOrEmpty(name))
            return Results.BadRequest("Backup file and name are required");

        if (name.Length > 100)
            return Results.BadRequest("Backup name too long (max 100 characters)");

        // Check file size (max 1GB)
        if (file.Length > 1024 * 1024 * 1024)
            return Results.BadRequest("Backup file too large (max 1GB)");

        // Check file extension
        if (!file.FileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest("Only .db files are allowed");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var backupData = memoryStream.ToArray();

        var success = await backupService.UploadBackupAsync(name, description, backupData);
        if (!success)
            return Results.Problem("Failed to upload backup");

        return Results.Ok(new { message = "Backup uploaded successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error uploading backup: {ex.Message}");
        return Results.Problem("Failed to upload backup");
    }
});

// Health Monitoring Endpoints
app.MapGet("/api/health", async (HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IHealthMonitoringService healthService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.health"))
        return Results.Forbid();

    try
    {
        var health = await healthService.GetSystemHealthAsync();
        return Results.Ok(health);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting system health: {ex.Message}");
        return Results.Problem("Failed to get system health");
    }
});

app.MapGet("/api/health/metrics", async (HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IHealthMonitoringService healthService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.health"))
        return Results.Forbid();

    try
    {
        var metrics = await healthService.GetSystemMetricsAsync();
        return Results.Ok(metrics);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting system metrics: {ex.Message}");
        return Results.Problem("Failed to get system metrics");
    }
});

app.MapGet("/api/health/services", async (HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IHealthMonitoringService healthService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.health"))
        return Results.Forbid();

    try
    {
        var services = await healthService.GetServiceStatusesAsync();
        return Results.Ok(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting service statuses: {ex.Message}");
        return Results.Problem("Failed to get service statuses");
    }
});

app.MapPost("/api/health/check/{serviceName}", async (string serviceName, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IHealthMonitoringService healthService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.health"))
        return Results.Forbid();

    try
    {
        var isHealthy = await healthService.CheckServiceHealthAsync(serviceName);
        return Results.Ok(new { serviceName, isHealthy, timestamp = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error checking service health: {ex.Message}");
        return Results.Problem("Failed to check service health");
    }
});

// Audit Trail Endpoints
app.MapGet("/api/audit/logs", async (
    [AsParameters] AuditLogFilter filter,
    HttpContext context, 
    IUserService userService, 
    IUserRoleService roleService,
    IAuditService auditService, 
    IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.audit"))
        return Results.Forbid();

    try
    {
        var response = await auditService.GetAuditLogsAsync(filter);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting audit logs: {ex.Message}");
        return Results.Problem("Failed to get audit logs");
    }
});

app.MapGet("/api/audit/actions", async (HttpContext context, IUserService userService, IUserRoleService roleService, IAuditService auditService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.audit"))
        return Results.Forbid();

    try
    {
        var actions = await auditService.GetAvailableActionsAsync();
        return Results.Ok(actions);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting available actions: {ex.Message}");
        return Results.Problem("Failed to get available actions");
    }
});

app.MapGet("/api/audit/resources", async (HttpContext context, IUserService userService, IUserRoleService roleService, IAuditService auditService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.audit"))
        return Results.Forbid();

    try
    {
        var resources = await auditService.GetAvailableResourcesAsync();
        return Results.Ok(resources);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting available resources: {ex.Message}");
        return Results.Problem("Failed to get available resources");
    }
});

// User Roles Endpoints
app.MapGet("/api/roles", async (HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    // Check if user has permission to view roles
    if (!await roleService.UserHasPermissionAsync(user.Id, "roles.view"))
        return Results.Forbid();

    try
    {
        var roles = await roleService.GetAllRolesAsync();
        return Results.Ok(roles);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting roles: {ex.Message}");
        return Results.Problem("Failed to get roles");
    }
});

app.MapGet("/api/roles/{roleId}", async (string roleId, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "roles.view"))
        return Results.Forbid();

    try
    {
        var role = await roleService.GetRoleByIdAsync(roleId);
        if (role == null)
            return Results.NotFound();

        return Results.Ok(role);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting role: {ex.Message}");
        return Results.Problem("Failed to get role");
    }
});

app.MapPost("/api/roles", async (CreateUserRoleRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "roles.create"))
        return Results.Forbid();

    try
    {
        var role = await roleService.CreateRoleAsync(request);
        return Results.Created($"/api/roles/{role.Id}", role);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating role: {ex.Message}");
        return Results.Problem("Failed to create role");
    }
});

app.MapPut("/api/roles/{roleId}", async (string roleId, UpdateUserRoleRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "roles.edit"))
        return Results.Forbid();

    try
    {
        var role = await roleService.UpdateRoleAsync(roleId, request);
        return Results.Ok(role);
    }
    catch (ArgumentException)
    {
        return Results.NotFound();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating role: {ex.Message}");
        return Results.Problem("Failed to update role");
    }
});

app.MapDelete("/api/roles/{roleId}", async (string roleId, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "roles.delete"))
        return Results.Forbid();

    try
    {
        var success = await roleService.DeleteRoleAsync(roleId);
        if (!success)
            return Results.NotFound();

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting role: {ex.Message}");
        return Results.Problem("Failed to delete role");
    }
});

app.MapPost("/api/roles/assign", async (AssignRoleRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "roles.assign"))
        return Results.Forbid();

    try
    {
        var success = await roleService.AssignRoleToUserAsync(request.UserId, request.RoleId);
        if (!success)
            return Results.NotFound();

        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error assigning role: {ex.Message}");
        return Results.Problem("Failed to assign role");
    }
});

app.MapGet("/api/roles/permissions", async (HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "roles.view"))
        return Results.Forbid();

    try
    {
        var permissions = await roleService.GetAvailablePermissionsAsync();
        return Results.Ok(permissions);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting permissions: {ex.Message}");
        return Results.Problem("Failed to get permissions");
    }
});

app.MapGet("/api/users/with-roles", async (HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    if (!await roleService.UserHasPermissionAsync(user.Id, "users.view"))
        return Results.Forbid();

    try
    {
        var users = await roleService.GetUsersWithRolesAsync();
        return Results.Ok(users);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting users with roles: {ex.Message}");
        return Results.Problem("Failed to get users with roles");
    }
});

// Guest Mode Endpoints
app.MapPost("/api/guest/create", async (CreateGuestRequest request, HttpContext context, IGuestService guestService) =>
{
    try
    {
        var guestUser = await guestService.CreateGuestUserAsync(request);
        return Results.Created($"/api/guest/{guestUser.SessionId}", guestUser);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating guest user: {ex.Message}");
        return Results.Problem("Failed to create guest user");
    }
});

app.MapGet("/api/guest/{sessionId}", async (string sessionId, IGuestService guestService) =>
{
    try
    {
        var guestUser = await guestService.GetGuestUserBySessionIdAsync(sessionId);
        if (guestUser == null)
            return Results.NotFound();

        return Results.Ok(guestUser);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting guest user: {ex.Message}");
        return Results.Problem("Failed to get guest user");
    }
});

app.MapPost("/api/guest/{sessionId}/extend", async (string sessionId, int hours, IGuestService guestService) =>
{
    try
    {
        var success = await guestService.ExtendGuestSessionAsync(sessionId, hours);
        if (!success)
            return Results.NotFound();

        return Results.Ok(new { success = true, message = $"Session extended by {hours} hours" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error extending guest session: {ex.Message}");
        return Results.Problem("Failed to extend guest session");
    }
});

app.MapPost("/api/guest/{sessionId}/deactivate", async (string sessionId, IGuestService guestService) =>
{
    try
    {
        var success = await guestService.DeactivateGuestUserAsync(sessionId);
        if (!success)
            return Results.NotFound();

        return Results.Ok(new { success = true, message = "Guest session deactivated" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deactivating guest session: {ex.Message}");
        return Results.Problem("Failed to deactivate guest session");
    }
});

app.MapPost("/api/guest/convert", async (ConvertGuestRequest request, IGuestService guestService) =>
{
    try
    {
        var success = await guestService.ConvertGuestToUserAsync(request.SessionId, request.Username, request.Password, request.Email);
        if (!success)
            return Results.BadRequest("Failed to convert guest to user. Username may already exist.");

        return Results.Ok(new { success = true, message = "Guest successfully converted to user" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error converting guest to user: {ex.Message}");
        return Results.Problem("Failed to convert guest to user");
    }
});

app.MapGet("/api/guest/active", async (HttpContext context, IUserService userService, IGuestService guestService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    // Only admins can view active guests
    if (user.Role != "Admin" && user.Role != "SuperAdmin")
        return Results.Forbid();

    try
    {
        var activeGuests = await guestService.GetActiveGuestsAsync();
        return Results.Ok(activeGuests);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting active guests: {ex.Message}");
        return Results.Problem("Failed to get active guests");
    }
});

app.MapPost("/api/guest/cleanup", async (GuestCleanupRequest request, HttpContext context, IUserService userService, IGuestService guestService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null)
        return Results.Unauthorized();

    // Only admins can cleanup guests
    if (user.Role != "Admin" && user.Role != "SuperAdmin")
        return Results.Forbid();

    try
    {
        var cleanedCount = await guestService.CleanupExpiredGuestsAsync(request.MaxAgeHours);
        return Results.Ok(new { success = true, cleanedCount, message = $"Cleaned up {cleanedCount} expired guest accounts" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error cleaning up guests: {ex.Message}");
        return Results.Problem("Failed to cleanup guests");
    }
});

// Webhook Management Endpoints
app.MapGet("/api/webhooks", async (HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IWebhookService webhookService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "webhooks.view"))
        return Results.Forbid();
    
    try
    {
        var webhooks = await webhookService.GetWebhooksAsync();
        return Results.Ok(webhooks);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting webhooks: {ex.Message}");
        return Results.Problem("Failed to get webhooks");
    }
});

app.MapGet("/api/webhooks/{id}", async (string id, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IWebhookService webhookService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "webhooks.view"))
        return Results.Forbid();
    
    try
    {
        var webhook = await webhookService.GetWebhookAsync(id);
        if (webhook == null) return Results.NotFound();
        return Results.Ok(webhook);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting webhook: {ex.Message}");
        return Results.Problem("Failed to get webhook");
    }
});

app.MapPost("/api/webhooks", async (CreateWebhookRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IWebhookService webhookService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "webhooks.create"))
        return Results.Forbid();
    
    try
    {
        var webhook = await webhookService.CreateWebhookAsync(request, user.Username);
        return Results.Created($"/api/webhooks/{webhook.Id}", webhook);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating webhook: {ex.Message}");
        return Results.Problem("Failed to create webhook");
    }
});

app.MapPut("/api/webhooks/{id}", async (string id, UpdateWebhookRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IWebhookService webhookService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "webhooks.edit"))
        return Results.Forbid();
    
    try
    {
        var webhook = await webhookService.UpdateWebhookAsync(id, request);
        return Results.Ok(webhook);
    }
    catch (ArgumentException)
    {
        return Results.NotFound();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating webhook: {ex.Message}");
        return Results.Problem("Failed to update webhook");
    }
});

app.MapDelete("/api/webhooks/{id}", async (string id, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IWebhookService webhookService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "webhooks.delete"))
        return Results.Forbid();
    
    try
    {
        await webhookService.DeleteWebhookAsync(id);
        return Results.NoContent();
    }
    catch (ArgumentException)
    {
        return Results.NotFound();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting webhook: {ex.Message}");
        return Results.Problem("Failed to delete webhook");
    }
});

app.MapPost("/api/webhooks/test", async (WebhookTestRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IWebhookService webhookService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "webhooks.test"))
        return Results.Forbid();
    
    try
    {
        var result = await webhookService.TestWebhookAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error testing webhook: {ex.Message}");
        return Results.Problem("Failed to test webhook");
    }
});

app.MapGet("/api/webhooks/{id}/deliveries", async (string id, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IWebhookService webhookService, int page = 1, int pageSize = 50) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "webhooks.view"))
        return Results.Forbid();
    
    try
    {
        var deliveries = await webhookService.GetWebhookDeliveriesAsync(id, page, pageSize);
        return Results.Ok(deliveries);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting webhook deliveries: {ex.Message}");
        return Results.Problem("Failed to get webhook deliveries");
    }
});

// End-to-End Encryption Endpoints
app.MapGet("/api/encryption/status", async (HttpContext context, IUserService userService, IJwtTokenService jwtService, IEncryptionService encryptionService, GatewayDbContext dbContext) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    try
    {
        var isEnabled = await encryptionService.IsEncryptionEnabledAsync(user.Id);
        var hasActiveKey = await dbContext.EncryptionKeys.AnyAsync(k => k.UserId == user.Id && k.IsActive);
        
        return Results.Ok(new EncryptionStatus(
            user.Id,
            isEnabled,
            hasActiveKey,
            user.EncryptionEnabledAt,
            user.LastKeyRotation,
            user.KeyRotationDays
        ));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting encryption status: {ex.Message}");
        return Results.Problem("Failed to get encryption status");
    }
});

app.MapPost("/api/encryption/enable", async (HttpContext context, IUserService userService, IJwtTokenService jwtService, IEncryptionService encryptionService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    try
    {
        var success = await encryptionService.EnableEncryptionAsync(user.Id);
        if (success)
        {
            return Results.Ok(new { success = true, message = "Encryption enabled successfully" });
        }
        else
        {
            return Results.Problem("Failed to enable encryption");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error enabling encryption: {ex.Message}");
        return Results.Problem("Failed to enable encryption");
    }
});

app.MapPost("/api/encryption/disable", async (HttpContext context, IUserService userService, IJwtTokenService jwtService, IEncryptionService encryptionService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    try
    {
        var success = await encryptionService.DisableEncryptionAsync(user.Id);
        if (success)
        {
            return Results.Ok(new { success = true, message = "Encryption disabled successfully" });
        }
        else
        {
            return Results.Problem("Failed to disable encryption");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error disabling encryption: {ex.Message}");
        return Results.Problem("Failed to disable encryption");
    }
});

app.MapPost("/api/encryption/rotate-key", async (HttpContext context, IUserService userService, IJwtTokenService jwtService, IEncryptionService encryptionService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    try
    {
        var success = await encryptionService.RotateEncryptionKeyAsync(user.Id);
        if (success)
        {
            return Results.Ok(new { success = true, message = "Encryption key rotated successfully" });
        }
        else
        {
            return Results.Problem("Failed to rotate encryption key");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error rotating encryption key: {ex.Message}");
        return Results.Problem("Failed to rotate encryption key");
    }
});

app.MapPut("/api/encryption/settings", async (UpdateEncryptionSettingsRequest request, HttpContext context, IUserService userService, IJwtTokenService jwtService, IEncryptionService encryptionService, GatewayDbContext dbContext) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    try
    {
        if (request.EncryptionEnabled.HasValue)
        {
            if (request.EncryptionEnabled.Value)
            {
                await encryptionService.EnableEncryptionAsync(user.Id);
            }
            else
            {
                await encryptionService.DisableEncryptionAsync(user.Id);
            }
        }
        
        // Update key rotation settings
        if (request.KeyRotationDays.HasValue)
        {
            user.KeyRotationDays = request.KeyRotationDays.Value;
            await dbContext.SaveChangesAsync();
        }
        
        return Results.Ok(new { success = true, message = "Encryption settings updated successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating encryption settings: {ex.Message}");
        return Results.Problem("Failed to update encryption settings");
    }
});

// Extended API Endpoints for External Applications
app.MapGet("/api/v1/status", () => Results.Ok(new { 
    status = "online", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    services = new { 
        database = "online",
        webhooks = "online",
        authentication = "online"
    }
}));

app.MapGet("/api/v1/info", () => Results.Ok(new {
    name = "LM Gateway API",
    version = "1.0.0",
    description = "API für das LM Gateway System",
    endpoints = new {
        authentication = "/api/auth/*",
        users = "/api/users/*",
        conversations = "/api/conversations/*",
        webhooks = "/api/webhooks/*",
        admin = "/api/admin/*",
        health = "/api/health/*"
    },
    rateLimits = new {
        global = "100 requests/minute",
        api = "1000 requests/minute"
    }
}));

app.MapGet("/api/v1/metrics", async (HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, GatewayDbContext dbContext) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "admin.dashboard"))
        return Results.Forbid();
    
    try
    {
        // Get basic stats from existing endpoints
        var totalUsers = await userService.GetAllUsersAsync();
        var totalConversations = await dbContext.Conversations.CountAsync();
        var totalMessages = await dbContext.Messages.CountAsync();
        
        return Results.Ok(new {
            timestamp = DateTime.UtcNow,
            metrics = new {
                users = new {
                    total = totalUsers.Count(),
                    active = totalUsers.Count(u => u.CreatedAt > DateTime.UtcNow.AddDays(-30)),
                    newToday = totalUsers.Count(u => u.CreatedAt.Date == DateTime.UtcNow.Date)
                },
                conversations = new {
                    total = totalConversations,
                    active = await dbContext.Conversations.CountAsync(c => c.UpdatedAt > DateTime.UtcNow.AddDays(-7)),
                    newToday = await dbContext.Conversations.CountAsync(c => c.CreatedAt.Date == DateTime.UtcNow.Date)
                },
                messages = new {
                    total = totalMessages,
                    newToday = await dbContext.Messages.CountAsync(m => m.CreatedAt.Date == DateTime.UtcNow.Date)
                }
            }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting metrics: {ex.Message}");
        return Results.Problem("Failed to get metrics");
    }
});

app.MapGet("/api/v1/export/users", async (HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "users.export"))
        return Results.Forbid();
    
    try
    {
        var users = await userService.GetAllUsersAsync();
        var exportData = users.Select(u => new {
            id = u.Id,
            username = u.Username,
            email = u.Email,
            role = u.Role,
            createdAt = u.CreatedAt,
            lastLogin = u.UpdatedAt,
            isActive = true
        });
        
        return Results.Ok(new {
            timestamp = DateTime.UtcNow,
            format = "json",
            count = exportData.Count(),
            data = exportData
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error exporting users: {ex.Message}");
        return Results.Problem("Failed to export users");
    }
});

app.MapGet("/api/v1/export/conversations", async (HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, GatewayDbContext dbContext) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "conversations.export"))
        return Results.Forbid();
    
    try
    {
        var conversations = await dbContext.Conversations
            .Include(c => c.Messages)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
            
        var exportData = conversations.Select(c => new {
            id = c.Id,
            title = c.Title,
            userId = c.UserId,
            model = c.Model,
            category = c.Category,
            createdAt = c.CreatedAt,
            updatedAt = c.UpdatedAt,
            messageCount = c.Messages.Count,
            messages = c.Messages.Select(m => new {
                id = m.Id,
                role = m.Role,
                content = m.Content,
                timestamp = m.CreatedAt
            })
        });
        
        return Results.Ok(new {
            timestamp = DateTime.UtcNow,
            format = "json",
            count = exportData.Count(),
            data = exportData
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error exporting conversations: {ex.Message}");
        return Results.Problem("Failed to export conversations");
    }
});

// Plugin Management Endpoints
app.MapGet("/api/plugins", async (HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IPluginManager pluginManager) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "plugins.view"))
        return Results.Forbid();
    
    try
    {
        var plugins = await pluginManager.GetLoadedPluginsAsync();
        var pluginInfos = plugins.Select(p => new PluginInfo(
            p.Name,
            p.Version,
            p.Description,
            p.Author,
            p.IsEnabled,
            DateTime.UtcNow // Vereinfacht
        ));
        
        return Results.Ok(pluginInfos);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting plugins: {ex.Message}");
        return Results.Problem("Failed to get plugins");
    }
});

app.MapGet("/api/plugins/{name}", async (string name, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IPluginManager pluginManager) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "plugins.view"))
        return Results.Forbid();
    
    try
    {
        var plugin = await pluginManager.GetPluginAsync(name);
        if (plugin == null) return Results.NotFound();
        
        var pluginInfo = new PluginInfo(
            plugin.Name,
            plugin.Version,
            plugin.Description,
            plugin.Author,
            plugin.IsEnabled,
            DateTime.UtcNow
        );
        
        return Results.Ok(pluginInfo);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting plugin: {ex.Message}");
        return Results.Problem("Failed to get plugin");
    }
});

app.MapPost("/api/plugins/{name}/enable", async (string name, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IPluginManager pluginManager) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "plugins.manage"))
        return Results.Forbid();
    
    try
    {
        await pluginManager.EnablePluginAsync(name);
        return Results.Ok(new { success = true, message = $"Plugin {name} enabled" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error enabling plugin: {ex.Message}");
        return Results.Problem("Failed to enable plugin");
    }
});

app.MapPost("/api/plugins/{name}/disable", async (string name, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IPluginManager pluginManager) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "plugins.manage"))
        return Results.Forbid();
    
    try
    {
        await pluginManager.DisablePluginAsync(name);
        return Results.Ok(new { success = true, message = $"Plugin {name} disabled" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error disabling plugin: {ex.Message}");
        return Results.Problem("Failed to disable plugin");
    }
});

app.MapPost("/api/plugins/{name}/reload", async (string name, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IPluginManager pluginManager) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "plugins.manage"))
        return Results.Forbid();
    
    try
    {
        await pluginManager.ReloadPluginAsync(name);
        return Results.Ok(new { success = true, message = $"Plugin {name} reloaded" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error reloading plugin: {ex.Message}");
        return Results.Problem("Failed to reload plugin");
    }
});

// Slack/Discord Integration Endpoints
app.MapGet("/api/integrations/slack/channels", async (HttpContext context, ISlackService slackService) =>
{
    try
    {
        var channels = await slackService.GetChannelsAsync();
        var isConfigured = slackService.IsConfigured;
        return Results.Ok(new { channels, configured = isConfigured });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting Slack channels: {ex.Message}");
        return Results.Problem("Failed to get Slack channels");
    }
}).AllowAnonymous();

app.MapPost("/api/integrations/slack/send", async (SlackSendRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, ISlackService slackService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "integrations.send"))
        return Results.Forbid();
    
    try
    {
        var success = await slackService.SendMessageAsync(request.Channel, request.Message, request.ThreadTs);
        return Results.Ok(new { success, message = success ? "Message sent successfully" : "Failed to send message" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending Slack message: {ex.Message}");
        return Results.Problem("Failed to send Slack message");
    }
});

app.MapGet("/api/integrations/discord/channels", async (HttpContext context, IDiscordService discordService) =>
{
    try
    {
        var channels = await discordService.GetChannelsAsync();
        var isConfigured = discordService.IsConfigured;
        return Results.Ok(new { channels, configured = isConfigured });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting Discord channels: {ex.Message}");
        return Results.Problem("Failed to get Discord channels");
    }
}).AllowAnonymous();

app.MapPost("/api/integrations/discord/send", async (DiscordSendRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IDiscordService discordService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "integrations.send"))
        return Results.Forbid();
    
    try
    {
        var success = await discordService.SendMessageAsync(request.ChannelId, request.Message);
        return Results.Ok(new { success, message = success ? "Message sent successfully" : "Failed to send message" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending Discord message: {ex.Message}");
        return Results.Problem("Failed to send Discord message");
    }
});

// Integration Configuration Endpoints
app.MapPost("/api/integrations/slack/configure", async (SlackConfigRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, ISlackService slackService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "integrations.manage"))
        return Results.Forbid();
    
    try
    {
        var success = await slackService.ConfigureAsync(request.BotToken, request.WebhookUrl);
        return Results.Ok(new { success, message = success ? "Slack configured successfully" : "Failed to configure Slack" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error configuring Slack: {ex.Message}");
        return Results.Problem("Failed to configure Slack");
    }
});

app.MapPost("/api/integrations/discord/configure", async (DiscordConfigRequest request, HttpContext context, IUserService userService, IUserRoleService roleService, IJwtTokenService jwtService, IDiscordService discordService) =>
{
    var user = await GetCurrentUserAsync(context, userService, jwtService);
    if (user == null) return Results.Unauthorized();
    
    if (!await roleService.UserHasPermissionAsync(user.Id, "integrations.manage"))
        return Results.Forbid();
    
    try
    {
        var success = await discordService.ConfigureAsync(request.BotToken, request.WebhookUrl);
        return Results.Ok(new { success, message = success ? "Discord configured successfully" : "Failed to configure Discord" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error configuring Discord: {ex.Message}");
        return Results.Problem("Failed to configure Discord");
    }
});

app.MapGet("/api/integrations/slack/status", async (HttpContext context, ISlackService slackService) =>
{
    try
    {
        var isConfigured = slackService.IsConfigured;
        var testResult = isConfigured ? await slackService.TestConnectionAsync() : false;
        return Results.Ok(new { configured = isConfigured, connected = testResult });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error checking Slack status: {ex.Message}");
        return Results.Problem("Failed to check Slack status");
    }
}).AllowAnonymous();

app.MapGet("/api/integrations/discord/status", async (HttpContext context, IDiscordService discordService) =>
{
    try
    {
        var isConfigured = discordService.IsConfigured;
        var testResult = isConfigured ? await discordService.TestConnectionAsync() : false;
        return Results.Ok(new { configured = isConfigured, connected = testResult });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error checking Discord status: {ex.Message}");
        return Results.Problem("Failed to check Discord status");
    }
}).AllowAnonymous();

// Initialize services and default data
await using var initScope = app.Services.CreateAsyncScope();
var initUserService = initScope.ServiceProvider.GetRequiredService<IUserService>();
var initUserRoleService = initScope.ServiceProvider.GetRequiredService<IUserRoleService>();
var initTemplateService = initScope.ServiceProvider.GetRequiredService<IChatTemplateService>();
var initHealthService = initScope.ServiceProvider.GetRequiredService<IHealthMonitoringService>();
var initPluginManager = initScope.ServiceProvider.GetRequiredService<IPluginManager>();

// Initialize default data
await initUserRoleService.InitializeDefaultRolesAsync(); // Initialize roles first
await initUserService.InitializeDefaultUsersAsync();
await initTemplateService.SeedBuiltInTemplatesAsync();
await initHealthService.StartHealthMonitoringAsync();
await initPluginManager.LoadPluginsAsync();

app.Run();