using System.Text.Json;
using SimpleGateway.Services;

namespace SimpleGateway.Plugins;

public class SlackIntegrationPlugin : INotificationPlugin
{
    private readonly ISlackService _slackService;
    
    public SlackIntegrationPlugin(ISlackService slackService)
    {
        _slackService = slackService;
    }
    
    public string Name => "Slack Integration Plugin";
    public string Version => "1.0.0";
    public string Description => "Integration with Slack for sending notifications";
    public string Author => "AIGS Team";
    public bool IsEnabled { get; set; } = true;

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task ShutdownAsync()
    {
        await Task.CompletedTask;
    }

    public async Task<bool> SendNotificationAsync(string channel, string title, string message, NotificationType type)
    {
        try
        {
            var fullMessage = $"{title}: {message}";
            return await _slackService.SendMessageAsync(channel, fullMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Slack integration error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsConfiguredAsync()
    {
        return await Task.FromResult(_slackService.IsConfigured);
    }

    public async Task<PluginInfo> GetInfoAsync()
    {
        return await Task.FromResult(new PluginInfo(
            Name,
            Version,
            Description,
            Author,
            IsEnabled,
            DateTime.UtcNow
        ));
    }
}

public class DiscordIntegrationPlugin : INotificationPlugin
{
    private readonly IDiscordService _discordService;
    
    public DiscordIntegrationPlugin(IDiscordService discordService)
    {
        _discordService = discordService;
    }
    
    public string Name => "Discord Integration Plugin";
    public string Version => "1.0.0";
    public string Description => "Integration with Discord for sending notifications";
    public string Author => "AIGS Team";
    public bool IsEnabled { get; set; } = true;

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task ShutdownAsync()
    {
        await Task.CompletedTask;
    }

    public async Task<bool> SendNotificationAsync(string channelId, string title, string message, NotificationType type)
    {
        try
        {
            var fullMessage = $"{title}: {message}";
            return await _discordService.SendMessageAsync(channelId, fullMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Discord integration error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsConfiguredAsync()
    {
        return await Task.FromResult(_discordService.IsConfigured);
    }

    public async Task<PluginInfo> GetInfoAsync()
    {
        return await Task.FromResult(new PluginInfo(
            Name,
            Version,
            Description,
            Author,
            IsEnabled,
            DateTime.UtcNow
        ));
    }
}
