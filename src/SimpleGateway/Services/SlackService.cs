using System.Text;
using System.Text.Json;

namespace SimpleGateway.Services;

public interface ISlackService
{
    Task<bool> SendMessageAsync(string channel, string message, string? threadTs = null);
    Task<string[]> GetChannelsAsync();
    Task<bool> TestConnectionAsync();
    Task<bool> ConfigureAsync(string botToken, string? webhookUrl = null);
    bool IsConfigured { get; }
}

public class SlackService : ISlackService
{
    private readonly HttpClient _httpClient;
    private string? _botToken;
    private string? _webhookUrl;
    
    public bool IsConfigured => !string.IsNullOrEmpty(_botToken) || !string.IsNullOrEmpty(_webhookUrl);

    public SlackService(HttpClient httpClient)
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
            Console.WriteLine($"Slack configuration error: {ex.Message}");
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
                    text = "Test message from LM Gateway"
                };
                
                var json = JsonSerializer.Serialize(testPayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(_webhookUrl, content);
                return response.IsSuccessStatusCode;
            }
            else if (!string.IsNullOrEmpty(_botToken))
            {
                // Test bot token with auth.test
                var request = new HttpRequestMessage(HttpMethod.Get, "https://slack.com/api/auth.test");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _botToken);
                
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(content);
                    return result.TryGetProperty("ok", out var ok) && ok.GetBoolean();
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Slack connection test error: {ex.Message}");
            return false;
        }
    }

    public async Task<string[]> GetChannelsAsync()
    {
        if (string.IsNullOrEmpty(_botToken))
            return new[] { "general", "random" }; // Default channels

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://slack.com/api/conversations.list?types=public_channel,private_channel");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _botToken);
            
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                
                if (result.TryGetProperty("channels", out var channels) && channels.ValueKind == JsonValueKind.Array)
                {
                    var channelList = new List<string>();
                    foreach (var channel in channels.EnumerateArray())
                    {
                        if (channel.TryGetProperty("name", out var name))
                        {
                            channelList.Add(name.GetString() ?? "");
                        }
                    }
                    return channelList.ToArray();
                }
            }
            
            return new[] { "general", "random" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Slack channels error: {ex.Message}");
            return new[] { "general", "random" };
        }
    }

    public async Task<bool> SendMessageAsync(string channel, string message, string? threadTs = null)
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
                    channel = channel,
                    text = message,
                    thread_ts = threadTs
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
                    channel = channel,
                    text = message,
                    thread_ts = threadTs
                };
                
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, "https://slack.com/api/chat.postMessage");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _botToken);
                request.Content = content;
                
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    return result.TryGetProperty("ok", out var ok) && ok.GetBoolean();
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Slack send message error: {ex.Message}");
            return false;
        }
    }
}
