using Gateway.Application.DTOs;
using Gateway.Application.Services;
using Gateway.Application.Validators;
using Gateway.Domain.Interfaces;
using Gateway.Infrastructure.Data;
using Gateway.Infrastructure.Repositories;
using Gateway.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddDbContext<GatewayDbContext>(options =>
{
    var provider = builder.Configuration["Database:Provider"] ?? "sqlite";
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=./data/gateway.db";

    if (provider.ToLower() == "sqlite")
    {
        options.UseSqlite(connectionString);
    }
    else if (provider.ToLower() == "postgresql")
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        throw new InvalidOperationException($"Unsupported database provider: {provider}");
    }
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUsageLogRepository, UsageLogRepository>();

builder.Services.AddHttpClient<ILmStudioClient, LmStudioClient>("LmStudioClient", client =>
{
    var baseUrl = builder.Configuration["LmStudio:BaseUrl"] ?? "http://127.0.0.1:1234";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("LmStudio:RequestTimeoutSeconds", 120));
});

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IConcurrencyManager, ConcurrencyManager>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ConversationService>();
builder.Services.AddScoped<ChatService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Security:JwtKey"]!);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Security:JwtIssuer"],
            ValidAudience = builder.Configuration["Security:JwtAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
        ?? new[] { "https://gateway.local", "http://localhost:5173" };
    
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LM Gateway API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.MapPost("/api/auth/login", async (LoginRequest request, AuthService authService) =>
{
    var validator = new LoginRequestValidator();
    var validationResult = await validator.ValidateAsync(request);
    
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
    }

    var result = await authService.LoginAsync(request);
    
    if (!result.Success)
    {
        return Results.Unauthorized();
    }

    var cookieName = builder.Configuration["Security:CookieName"] ?? "access_token";
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = app.Environment.IsProduction(),
        SameSite = SameSiteMode.Lax,
        Expires = DateTime.UtcNow.AddHours(24)
    };

    return Results.Ok().WithCookie(cookieName, result.Token, cookieOptions);
})
.WithName("Login")
.WithOpenApi();

app.MapPost("/api/auth/logout", () =>
{
    var cookieName = builder.Configuration["Security:CookieName"] ?? "access_token";
    return Results.NoContent().WithCookie(cookieName, "", new CookieOptions { Expires = DateTime.UtcNow.AddDays(-1) });
})
.WithName("Logout")
.WithOpenApi();

app.MapGet("/api/auth/csrf", () =>
{
    var csrfToken = Guid.NewGuid().ToString();
    return Results.Ok(new CsrfResponse(csrfToken));
})
.WithName("GetCsrfToken")
.WithOpenApi();

app.MapGet("/api/models", async (ChatService chatService) =>
{
    var models = await chatService.GetModelsAsync();
    return Results.Ok(models);
})
.WithName("GetModels")
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/api/chat", async (
    ChatRequest request,
    HttpContext context,
    ChatService chatService,
    ConversationService conversationService) =>
{
    var validator = new ChatRequestValidator();
    var validationResult = await validator.ValidateAsync(request);
    
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
    }

    var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (userId == null || !Guid.TryParse(userId, out var userIdGuid))
    {
        return Results.Unauthorized();
    }

    var conversationId = Guid.NewGuid();
    var conversationTitle = request.Messages.FirstOrDefault(m => m.Role == "user")?.Content?[..Math.Min(50, request.Messages.FirstOrDefault(m => m.Role == "user")?.Content?.Length ?? 0)] ?? "New Conversation";
    
    await conversationService.CreateConversationAsync(userIdGuid, new CreateConversationRequest(conversationTitle));

    var stream = await chatService.ProcessChatAsync(userIdGuid, conversationId, request);

    return Results.Stream(stream, "text/event-stream", new Dictionary<string, string>
    {
        ["Cache-Control"] = "no-cache",
        ["Connection"] = "keep-alive"
    });
})
.WithName("Chat")
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/api/conversations", async (CreateConversationRequest request, HttpContext context, ConversationService conversationService) =>
{
    var validator = new CreateConversationRequestValidator();
    var validationResult = await validator.ValidateAsync(request);
    
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
    }

    var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (userId == null || !Guid.TryParse(userId, out var userIdGuid))
    {
        return Results.Unauthorized();
    }

    var conversation = await conversationService.CreateConversationAsync(userIdGuid, request);
    return Results.Ok(conversation);
})
.WithName("CreateConversation")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/conversations", async (HttpContext context, ConversationService conversationService) =>
{
    var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (userId == null || !Guid.TryParse(userId, out var userIdGuid))
    {
        return Results.Unauthorized();
    }

    var conversations = await conversationService.GetConversationsAsync(userIdGuid);
    return Results.Ok(conversations);
})
.WithName("GetConversations")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/conversations/{id:guid}", async (Guid id, HttpContext context, ConversationService conversationService) =>
{
    var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (userId == null || !Guid.TryParse(userId, out var userIdGuid))
    {
        return Results.Unauthorized();
    }

    var conversation = await conversationService.GetConversationWithMessagesAsync(id, userIdGuid);
    if (conversation == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(conversation);
})
.WithName("GetConversation")
.WithOpenApi()
.RequireAuthorization();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
    await context.Database.MigrateAsync();
    
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var adminUser = await userRepository.GetByUsernameAsync("admin");
    if (adminUser == null)
    {
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "admin";
        var admin = new Gateway.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            DailyTokenQuota = 1000000
        };
        
        await userRepository.CreateAsync(admin);
        Log.Information("Created default admin user");
    }
}

app.Run();