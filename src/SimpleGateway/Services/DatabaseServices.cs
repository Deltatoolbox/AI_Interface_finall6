using Microsoft.EntityFrameworkCore;
using SimpleGateway.Data;
using SimpleGateway.Models;
using SimpleGateway.DTOs;
using BCrypt.Net;

namespace SimpleGateway.Services;

public interface IUserService
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> CreateUserAsync(string username, string password, string email = "", string role = "User");
    Task<bool> ValidatePasswordAsync(User user, string password);
    Task<bool> ValidatePasswordAsync(string userId, string password);
    Task<List<User>> GetAllUsersAsync();
    Task<User?> UpdateUserAsync(string userId, string? username, string? email, string? role);
    Task<bool> UpdatePasswordAsync(string userId, string newPassword);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> UserExistsAsync(string username);
}

public interface IConversationService
{
    Task<ConversationResponse[]> GetConversationsByUserIdAsync(string userId);
    Task<ConversationResponse> CreateConversationAsync(string userId, string title);
    Task<ConversationWithMessagesResponse?> GetConversationWithMessagesAsync(string conversationId, string userId);
    Task<ConversationResponse?> UpdateConversationTitleAsync(string conversationId, string userId, string newTitle);
    Task<bool> DeleteAllConversationsForUserAsync(string userId);
}

public interface IMessageService
{
    Task SaveMessagesAsync(string conversationId, MessageDto[] messages);
    Task SaveAssistantMessageAsync(string conversationId, string content);
}

public class UserService : IUserService
{
    private readonly GatewayDbContext _context;

    public UserService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> CreateUserAsync(string username, string password, string email = "", string role = "User")
    {
        var existingUser = await GetUserByUsernameAsync(username);
        if (existingUser != null)
            return null;

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Email = email,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ValidatePasswordAsync(User user, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<bool> ValidatePasswordAsync(string userId, string password)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return false;
        
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<User?> UpdateUserAsync(string userId, string? username, string? email, string? role)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return null;

        if (!string.IsNullOrEmpty(username))
            user.Username = username;
        if (!string.IsNullOrEmpty(email))
            user.Email = email;
        if (!string.IsNullOrEmpty(role))
            user.Role = role;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username);
    }

    public async Task<bool> UpdatePasswordAsync(string userId, string newPassword)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}

public class ConversationService : IConversationService
{
    private readonly GatewayDbContext _context;

    public ConversationService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task<ConversationResponse[]> GetConversationsByUserIdAsync(string userId)
    {
        var conversations = await _context.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new ConversationResponse(c.Id, c.Title, c.CreatedAt, c.UpdatedAt))
            .ToArrayAsync();

        return conversations;
    }

    public async Task<ConversationResponse> CreateConversationAsync(string userId, string title)
    {
        var conversation = new Conversation
        {
            Title = title,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        return new ConversationResponse(conversation.Id, conversation.Title, conversation.CreatedAt, conversation.UpdatedAt);
    }

    public async Task<ConversationWithMessagesResponse?> GetConversationWithMessagesAsync(string conversationId, string userId)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

        if (conversation == null)
            return null;

        var messages = conversation.Messages.Select(m => 
            new MessageResponse(m.Id, m.Role, m.Content, m.CreatedAt)).ToArray();

        return new ConversationWithMessagesResponse(
            conversation.Id, 
            conversation.Title, 
            conversation.CreatedAt, 
            conversation.UpdatedAt, 
            messages);
    }

    public async Task<ConversationResponse?> UpdateConversationTitleAsync(string conversationId, string userId, string newTitle)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

        if (conversation == null)
            return null;

        conversation.Title = newTitle;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ConversationResponse(conversation.Id, conversation.Title, conversation.CreatedAt, conversation.UpdatedAt);
    }

    public async Task<bool> DeleteAllConversationsForUserAsync(string userId)
    {
        var conversations = await _context.Conversations
            .Where(c => c.UserId == userId)
            .Include(c => c.Messages)
            .ToListAsync();

        if (!conversations.Any())
            return true;

        // Delete all messages first (due to foreign key constraints)
        foreach (var conversation in conversations)
        {
            _context.Messages.RemoveRange(conversation.Messages);
        }

        // Then delete conversations
        _context.Conversations.RemoveRange(conversations);
        
        await _context.SaveChangesAsync();
        return true;
    }
}

public class MessageService : IMessageService
{
    private readonly GatewayDbContext _context;

    public MessageService(GatewayDbContext context)
    {
        _context = context;
    }

    public async Task SaveMessagesAsync(string conversationId, MessageDto[] messages)
    {
        var messageEntities = messages.Select(m => new Message
        {
            ConversationId = conversationId,
            Role = m.Role,
            Content = m.Content,
            CreatedAt = DateTime.UtcNow
        }).ToArray();

        _context.Messages.AddRange(messageEntities);

        // Update conversation timestamp
        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation != null)
        {
            conversation.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SaveAssistantMessageAsync(string conversationId, string content)
    {
        var message = new Message
        {
            ConversationId = conversationId,
            Role = "assistant",
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);

        // Update conversation timestamp
        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation != null)
        {
            conversation.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
