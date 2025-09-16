using System.Text.RegularExpressions;

namespace SimpleGateway.Plugins
{
    /// <summary>
    /// Beispiel-Plugin für Text-Verarbeitung (Markdown, Emoji, etc.)
    /// </summary>
    public class TextProcessingPlugin : IChatPlugin
    {
        public string Name => "Text Processing Plugin";
        public string Version => "1.0.0";
        public string Description => "Erweitert Nachrichten mit Markdown-Unterstützung und Emoji-Verarbeitung";
        public string Author => "LM Gateway Team";
        public bool IsEnabled { get; set; } = true;

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task ShutdownAsync()
        {
            await Task.CompletedTask;
        }

        public async Task<string> ProcessMessageAsync(string message, string userId, string conversationId)
        {
            if (!IsEnabled) return message;

            // Markdown-Verarbeitung
            var processedMessage = ProcessMarkdown(message);
            
            // Emoji-Verarbeitung
            processedMessage = ProcessEmojis(processedMessage);
            
            // Link-Verarbeitung
            processedMessage = ProcessLinks(processedMessage);
            
            return await Task.FromResult(processedMessage);
        }

        public async Task<string[]> GetAvailableTemplatesAsync()
        {
            return await Task.FromResult(new[]
            {
                "markdown-template",
                "emoji-template",
                "link-template"
            });
        }

        public async Task<string> ProcessTemplateAsync(string templateName, Dictionary<string, object> parameters)
        {
            return templateName switch
            {
                "markdown-template" => ProcessMarkdown(parameters.GetValueOrDefault("content", "").ToString() ?? ""),
                "emoji-template" => ProcessEmojis(parameters.GetValueOrDefault("content", "").ToString() ?? ""),
                "link-template" => ProcessLinks(parameters.GetValueOrDefault("content", "").ToString() ?? ""),
                _ => parameters.GetValueOrDefault("content", "").ToString() ?? ""
            };
        }

        private string ProcessMarkdown(string text)
        {
            // Einfache Markdown-Verarbeitung
            text = Regex.Replace(text, @"\*\*(.*?)\*\*", "<strong>$1</strong>");
            text = Regex.Replace(text, @"\*(.*?)\*", "<em>$1</em>");
            text = Regex.Replace(text, @"`(.*?)`", "<code>$1</code>");
            return text;
        }

        private string ProcessEmojis(string text)
        {
            // Emoji-Verarbeitung
            var emojiMap = new Dictionary<string, string>
            {
                { ":)", "😊" },
                { ":(", "😢" },
                { ":D", "😄" },
                { ":P", "😛" },
                { ":heart:", "❤️" },
                { ":thumbsup:", "👍" },
                { ":thumbsdown:", "👎" }
            };

            foreach (var emoji in emojiMap)
            {
                text = text.Replace(emoji.Key, emoji.Value);
            }

            return text;
        }

        private string ProcessLinks(string text)
        {
            // Link-Verarbeitung
            var urlPattern = @"(https?://[^\s]+)";
            return Regex.Replace(text, urlPattern, "<a href=\"$1\" target=\"_blank\">$1</a>");
        }
    }

    /// <summary>
    /// Beispiel-Plugin für Analytics und Usage-Tracking
    /// </summary>
    public class AnalyticsPlugin : IAnalyticsPlugin
    {
        public string Name => "Analytics Plugin";
        public string Version => "1.0.0";
        public string Description => "Sammelt und analysiert Nutzungsdaten";
        public string Author => "LM Gateway Team";
        public bool IsEnabled { get; set; } = true;

        private readonly Dictionary<string, int> _eventCounts = new();
        private readonly Dictionary<string, List<DateTime>> _eventTimestamps = new();

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task ShutdownAsync()
        {
            await Task.CompletedTask;
        }

        public async Task TrackEventAsync(string eventName, Dictionary<string, object> properties)
        {
            if (!IsEnabled) return;

            lock (_eventCounts)
            {
                _eventCounts[eventName] = _eventCounts.GetValueOrDefault(eventName, 0) + 1;
                
                if (!_eventTimestamps.ContainsKey(eventName))
                {
                    _eventTimestamps[eventName] = new List<DateTime>();
                }
                _eventTimestamps[eventName].Add(DateTime.UtcNow);
            }

            await Task.CompletedTask;
        }

        public async Task<Dictionary<string, object>> GetMetricsAsync(DateTime from, DateTime to)
        {
            var metrics = new Dictionary<string, object>();

            lock (_eventCounts)
            {
                foreach (var eventName in _eventCounts.Keys)
                {
                    var count = _eventCounts[eventName];
                    var timestamps = _eventTimestamps.GetValueOrDefault(eventName, new List<DateTime>());
                    var recentCount = timestamps.Count(t => t >= from && t <= to);

                    metrics[eventName] = new
                    {
                        totalCount = count,
                        recentCount = recentCount,
                        averagePerDay = timestamps.Count > 0 ? (double)count / (DateTime.UtcNow - timestamps.Min()).TotalDays : 0
                    };
                }
            }

            return await Task.FromResult(metrics);
        }
    }

    /// <summary>
    /// Beispiel-Plugin für File-Storage (lokaler Speicher)
    /// </summary>
    public class LocalStoragePlugin : IStoragePlugin
    {
        public string Name => "Local Storage Plugin";
        public string Version => "1.0.0";
        public string Description => "Speichert Dateien lokal auf dem Server";
        public string Author => "LM Gateway Team";
        public bool IsEnabled { get; set; } = true;

        private readonly string _storageDirectory;

        public LocalStoragePlugin()
        {
            _storageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads");
            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task ShutdownAsync()
        {
            await Task.CompletedTask;
        }

        public async Task<string> UploadFileAsync(byte[] fileData, string fileName, string contentType)
        {
            if (!IsEnabled) throw new InvalidOperationException("Plugin is disabled");

            var fileId = Guid.NewGuid().ToString();
            var filePath = Path.Combine(_storageDirectory, $"{fileId}_{fileName}");
            
            await File.WriteAllBytesAsync(filePath, fileData);
            
            return await Task.FromResult(fileId);
        }

        public async Task<byte[]> DownloadFileAsync(string fileId)
        {
            if (!IsEnabled) throw new InvalidOperationException("Plugin is disabled");

            var files = Directory.GetFiles(_storageDirectory, $"{fileId}_*");
            if (files.Length == 0)
            {
                throw new FileNotFoundException($"File with ID {fileId} not found");
            }

            return await File.ReadAllBytesAsync(files[0]);
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            if (!IsEnabled) return false;

            var files = Directory.GetFiles(_storageDirectory, $"{fileId}_*");
            if (files.Length == 0) return false;

            File.Delete(files[0]);
            return await Task.FromResult(true);
        }

        public async Task<FileInfo> GetFileInfoAsync(string fileId)
        {
            if (!IsEnabled) throw new InvalidOperationException("Plugin is disabled");

            var files = Directory.GetFiles(_storageDirectory, $"{fileId}_*");
            if (files.Length == 0)
            {
                throw new FileNotFoundException($"File with ID {fileId} not found");
            }

            var filePath = files[0];
            var fileInfo = new System.IO.FileInfo(filePath);
            var fileName = filePath.Substring(filePath.LastIndexOf('_') + 1);

            return await Task.FromResult(new FileInfo(
                fileId,
                fileName,
                fileInfo.Length,
                "application/octet-stream", // Vereinfacht
                fileInfo.CreationTimeUtc
            ));
        }
    }
}
