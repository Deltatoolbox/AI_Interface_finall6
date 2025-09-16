using System.Reflection;

namespace SimpleGateway.Plugins
{
    /// <summary>
    /// Basis-Interface für alle Plugins
    /// </summary>
    public interface IPlugin
    {
        string Name { get; }
        string Version { get; }
        string Description { get; }
        string Author { get; }
        bool IsEnabled { get; set; }
        Task InitializeAsync();
        Task ShutdownAsync();
    }

    /// <summary>
    /// Interface für Chat-Plugins (Message-Processing, Templates, etc.)
    /// </summary>
    public interface IChatPlugin : IPlugin
    {
        Task<string> ProcessMessageAsync(string message, string userId, string conversationId);
        Task<string[]> GetAvailableTemplatesAsync();
        Task<string> ProcessTemplateAsync(string templateName, Dictionary<string, object> parameters);
    }

    /// <summary>
    /// Interface für Authentication-Plugins (SSO, OAuth, etc.)
    /// </summary>
    public interface IAuthPlugin : IPlugin
    {
        Task<AuthResult> AuthenticateAsync(string credentials);
        Task<bool> IsValidTokenAsync(string token);
        Task<UserInfo> GetUserInfoAsync(string token);
    }

    /// <summary>
    /// Interface für Notification-Plugins (Email, SMS, Push, etc.)
    /// </summary>
    public interface INotificationPlugin : IPlugin
    {
        Task<bool> SendNotificationAsync(string recipient, string subject, string message, NotificationType type);
        Task<bool> IsConfiguredAsync();
    }

    /// <summary>
    /// Interface für Storage-Plugins (File-Upload, Cloud-Storage, etc.)
    /// </summary>
    public interface IStoragePlugin : IPlugin
    {
        Task<string> UploadFileAsync(byte[] fileData, string fileName, string contentType);
        Task<byte[]> DownloadFileAsync(string fileId);
        Task<bool> DeleteFileAsync(string fileId);
        Task<FileInfo> GetFileInfoAsync(string fileId);
    }

    /// <summary>
    /// Interface für Analytics-Plugins (Usage-Tracking, Metrics, etc.)
    /// </summary>
    public interface IAnalyticsPlugin : IPlugin
    {
        Task TrackEventAsync(string eventName, Dictionary<string, object> properties);
        Task<Dictionary<string, object>> GetMetricsAsync(DateTime from, DateTime to);
    }

    /// <summary>
    /// Plugin-Manager für das Laden und Verwalten von Plugins
    /// </summary>
    public interface IPluginManager
    {
        Task LoadPluginsAsync();
        Task UnloadPluginAsync(string pluginName);
        Task ReloadPluginAsync(string pluginName);
        Task<IEnumerable<IPlugin>> GetLoadedPluginsAsync();
        Task<IPlugin?> GetPluginAsync(string pluginName);
        Task EnablePluginAsync(string pluginName);
        Task DisablePluginAsync(string pluginName);
    }

    // DTOs für Plugin-System
    public record AuthResult(bool Success, string? Token, string? ErrorMessage, UserInfo? UserInfo);
    public record UserInfo(string Id, string Username, string Email, string[] Roles);
    public record FileInfo(string Id, string Name, long Size, string ContentType, DateTime UploadedAt);
    
    public enum NotificationType
    {
        Email,
        SMS,
        Push,
        Webhook
    }

    public record PluginInfo(string Name, string Version, string Description, string Author, bool IsEnabled, DateTime LoadedAt);
}
