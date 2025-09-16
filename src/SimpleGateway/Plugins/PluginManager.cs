using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleGateway.Data;

namespace SimpleGateway.Plugins
{
    public class PluginManager : IPluginManager
    {
        private readonly Dictionary<string, IPlugin> _loadedPlugins = new();
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PluginManager> _logger;
        private readonly GatewayDbContext _context;
        private readonly string _pluginsDirectory;

        public PluginManager(IServiceProvider serviceProvider, ILogger<PluginManager> logger, GatewayDbContext context)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _context = context;
            _pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
            
            // Erstelle Plugins-Verzeichnis falls es nicht existiert
            if (!Directory.Exists(_pluginsDirectory))
            {
                Directory.CreateDirectory(_pluginsDirectory);
            }
        }

        public async Task LoadPluginsAsync()
        {
            try
            {
                _logger.LogInformation("Loading plugins from directory: {PluginsDirectory}", _pluginsDirectory);
                
                var pluginFiles = Directory.GetFiles(_pluginsDirectory, "*.dll");
                
                foreach (var pluginFile in pluginFiles)
                {
                    try
                    {
                        await LoadPluginFromFileAsync(pluginFile);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to load plugin from file: {PluginFile}", pluginFile);
                    }
                }
                
                _logger.LogInformation("Loaded {PluginCount} plugins", _loadedPlugins.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading plugins");
            }
        }

        private async Task LoadPluginFromFileAsync(string pluginFile)
        {
            var assembly = Assembly.LoadFrom(pluginFile);
            _loadedAssemblies[Path.GetFileNameWithoutExtension(pluginFile)] = assembly;
            
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            
            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    var plugin = ActivatorUtilities.CreateInstance(_serviceProvider, pluginType) as IPlugin;
                    if (plugin != null)
                    {
                        await plugin.InitializeAsync();
                        _loadedPlugins[plugin.Name] = plugin;
                        _logger.LogInformation("Loaded plugin: {PluginName} v{Version}", plugin.Name, plugin.Version);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create instance of plugin type: {PluginType}", pluginType.Name);
                }
            }
        }

        public async Task UnloadPluginAsync(string pluginName)
        {
            if (_loadedPlugins.TryGetValue(pluginName, out var plugin))
            {
                try
                {
                    await plugin.ShutdownAsync();
                    _loadedPlugins.Remove(pluginName);
                    _logger.LogInformation("Unloaded plugin: {PluginName}", pluginName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error unloading plugin: {PluginName}", pluginName);
                }
            }
        }

        public async Task ReloadPluginAsync(string pluginName)
        {
            await UnloadPluginAsync(pluginName);
            
            // Finde die entsprechende DLL-Datei
            var assemblyName = _loadedAssemblies.Keys.FirstOrDefault(k => 
                _loadedPlugins.ContainsKey(pluginName) && 
                _loadedPlugins[pluginName].GetType().Assembly == _loadedAssemblies[k]);
            
            if (assemblyName != null)
            {
                var pluginFile = Path.Combine(_pluginsDirectory, $"{assemblyName}.dll");
                await LoadPluginFromFileAsync(pluginFile);
            }
        }

        public Task<IEnumerable<IPlugin>> GetLoadedPluginsAsync()
        {
            return Task.FromResult(_loadedPlugins.Values.AsEnumerable());
        }

        public Task<IPlugin?> GetPluginAsync(string pluginName)
        {
            _loadedPlugins.TryGetValue(pluginName, out var plugin);
            return Task.FromResult(plugin);
        }

        public async Task EnablePluginAsync(string pluginName)
        {
            if (_loadedPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.IsEnabled = true;
                await plugin.InitializeAsync();
                _logger.LogInformation("Enabled plugin: {PluginName}", pluginName);
            }
        }

        public async Task DisablePluginAsync(string pluginName)
        {
            if (_loadedPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.IsEnabled = false;
                await plugin.ShutdownAsync();
                _logger.LogInformation("Disabled plugin: {PluginName}", pluginName);
            }
        }
    }
}
