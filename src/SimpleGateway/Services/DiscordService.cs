using System.Text;
using System.Text.Json;

namespace SimpleGateway.Services;

public interface IDiscordService
{
    Task<bool> SendMessageAsync(string channelId, string message);
    Task<string[]> GetChannelsAsync();
    Task<bool> TestConnectionAsync();
    Task<bool> ConfigureAsync(string botToken, string? webhookUrl = null);
    bool IsConfigured { get; }
}

public class DiscordService : IDiscordService
{
    private readonly HttpClient _httpClient;
    private string? _botToken;
    private string? _webhookUrl;
    
    public bool IsConfigured => !string.IsNullOrEmpty(_botToken) || !string.IsNullOrEmpty(_webhookUrl);

    public DiscordService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ConfigureAsync(string botToken, string? webhookUrl = null)
    {
        try
        {
            _botToken = botToken;
            _webhookUrl = webhookUrl;
            
            // Test the configuration
            return await TestConnectionAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Discord configuration error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        if (!IsConfigured)
            return false;

        try
        {
            if (!string.IsNullOrEmpty(_webhookUrl))
            {
                // Test webhook
                var testPayload = new
                {
                    content = "Test message from LM Gateway"
                };
                
                var json = JsonSerializer.Serialize(testPayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(_webhookUrl, content);
                return response.IsSuccessStatusCode;
            }
            else if (!string.IsNullOrEmpty(_botToken))
            {
                // Test bot token with @me endpoint
                var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/v10/users/@me");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bot", _botToken);
                
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Discord connection test error: {ex.Message}");
            return false;
        }
    }

    public async Task<string[]> GetChannelsAsync()
    {
        if (string.IsNullOrEmpty(_botToken))
            return new[] { "general", "bot-commands" }; // Default channels

        try
        {
            // Note: This requires the bot to be in a guild and have permissions
            // For now, return default channels
            return new[] { "general", "bot-commands" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Discord channels error: {ex.Message}");
            return new[] { "general", "bot-commands" };
        }
    }

    public async Task<bool> SendMessageAsync(string channelId, string message)
    {
        if (!IsConfigured)
            return false;

        try
        {
            if (!string.IsNullOrEmpty(_webhookUrl))
            {
                // Use webhook
                var payload = new
                {
                    content = message
                };
                
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(_webhookUrl, content);
                return response.IsSuccessStatusCode;
            }
            else if (!string.IsNullOrEmpty(_botToken))
            {
                // Use Bot API
                var payload = new
                {
                    content = message
                };
                
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"https://discord.com/api/v10/channels/{channelId}/messages");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bot", _botToken);
                request.Content = content;
                
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Discord send message error: {ex.Message}");
            return false;
        }
    }
}
