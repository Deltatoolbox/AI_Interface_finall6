using Gateway.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Gateway.Infrastructure.Services;

public sealed class LmStudioClient : ILmStudioClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LmStudioClient> _logger;

    public LmStudioClient(HttpClient httpClient, IMemoryCache cache, ILogger<LmStudioClient> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Gets available models from LM Studio with caching
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available models</returns>
    public async Task<IEnumerable<LmModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "lm_studio_models";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<LmModel>? cachedModels))
        {
            return cachedModels!;
        }

        try
        {
            var response = await _httpClient.GetAsync("/v1/models", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var modelsResponse = JsonSerializer.Deserialize<ModelsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var models = modelsResponse?.Data?.Select(m => new LmModel(m.Id, m.Object, m.Created, m.OwnedBy)) ?? Enumerable.Empty<LmModel>();

            _cache.Set(cacheKey, models, TimeSpan.FromMinutes(1));
            
            _logger.LogInformation("Retrieved {Count} models from LM Studio", models.Count());
            
            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve models from LM Studio");
            throw;
        }
    }

    /// <summary>
    /// Sends a chat completion request to LM Studio and returns streaming response
    /// </summary>
    /// <param name="request">Chat completion request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Streaming response</returns>
    public async Task<Stream> ChatCompletionStreamAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/v1/chat/completions", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Started chat completion stream for model {Model}", request.Model);
            
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start chat completion stream for model {Model}", request.Model);
            throw;
        }
    }

    private record ModelsResponse(LmModelData[]? Data);
    private record LmModelData(string Id, string Object, long Created, string OwnedBy);
}
