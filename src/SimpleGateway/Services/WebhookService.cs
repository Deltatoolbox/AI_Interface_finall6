using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using SimpleGateway.Data;
using SimpleGateway.Models;
using SimpleGateway.DTOs;

namespace SimpleGateway.Services
{
    public interface IWebhookService
    {
        Task<IEnumerable<WebhookDto>> GetWebhooksAsync();
        Task<WebhookDto?> GetWebhookAsync(string id);
        Task<WebhookDto> CreateWebhookAsync(CreateWebhookRequest request, string createdBy);
        Task<WebhookDto> UpdateWebhookAsync(string id, UpdateWebhookRequest request);
        Task DeleteWebhookAsync(string id);
        Task<WebhookTestResponse> TestWebhookAsync(WebhookTestRequest request);
        Task<IEnumerable<WebhookDeliveryDto>> GetWebhookDeliveriesAsync(string webhookId, int page = 1, int pageSize = 50);
        Task TriggerWebhookAsync(string eventType, object payload);
        Task ProcessPendingDeliveriesAsync();
    }

    public class WebhookService : IWebhookService
    {
        private readonly GatewayDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WebhookService> _logger;

        public WebhookService(GatewayDbContext context, HttpClient httpClient, ILogger<WebhookService> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<WebhookDto>> GetWebhooksAsync()
        {
            var webhooks = await _context.Webhooks
                .OrderBy(w => w.Name)
                .ToListAsync();

            return webhooks.Select(w => new WebhookDto(
                w.Id,
                w.Name,
                w.Url,
                w.Events,
                w.IsActive,
                w.RetryCount,
                w.TimeoutSeconds,
                w.CreatedAt,
                w.UpdatedAt,
                w.Description,
                w.CreatedBy
            ));
        }

        public async Task<WebhookDto?> GetWebhookAsync(string id)
        {
            var webhook = await _context.Webhooks.FindAsync(id);
            if (webhook == null) return null;

            return new WebhookDto(
                webhook.Id,
                webhook.Name,
                webhook.Url,
                webhook.Events,
                webhook.IsActive,
                webhook.RetryCount,
                webhook.TimeoutSeconds,
                webhook.CreatedAt,
                webhook.UpdatedAt,
                webhook.Description,
                webhook.CreatedBy
            );
        }

        public async Task<WebhookDto> CreateWebhookAsync(CreateWebhookRequest request, string createdBy)
        {
            var webhook = new Webhook
            {
                Name = request.Name,
                Url = request.Url,
                Secret = request.Secret,
                Events = JsonSerializer.Serialize(request.Events),
                Description = request.Description,
                RetryCount = request.RetryCount,
                TimeoutSeconds = request.TimeoutSeconds,
                CreatedBy = createdBy
            };

            _context.Webhooks.Add(webhook);
            await _context.SaveChangesAsync();

            return new WebhookDto(
                webhook.Id,
                webhook.Name,
                webhook.Url,
                webhook.Events,
                webhook.IsActive,
                webhook.RetryCount,
                webhook.TimeoutSeconds,
                webhook.CreatedAt,
                webhook.UpdatedAt,
                webhook.Description,
                webhook.CreatedBy
            );
        }

        public async Task<WebhookDto> UpdateWebhookAsync(string id, UpdateWebhookRequest request)
        {
            var webhook = await _context.Webhooks.FindAsync(id);
            if (webhook == null) throw new ArgumentException("Webhook not found");

            if (request.Name != null) webhook.Name = request.Name;
            if (request.Url != null) webhook.Url = request.Url;
            if (request.Secret != null) webhook.Secret = request.Secret;
            if (request.Events != null) webhook.Events = JsonSerializer.Serialize(request.Events);
            if (request.Description != null) webhook.Description = request.Description;
            if (request.IsActive.HasValue) webhook.IsActive = request.IsActive.Value;
            if (request.RetryCount.HasValue) webhook.RetryCount = request.RetryCount.Value;
            if (request.TimeoutSeconds.HasValue) webhook.TimeoutSeconds = request.TimeoutSeconds.Value;

            webhook.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new WebhookDto(
                webhook.Id,
                webhook.Name,
                webhook.Url,
                webhook.Events,
                webhook.IsActive,
                webhook.RetryCount,
                webhook.TimeoutSeconds,
                webhook.CreatedAt,
                webhook.UpdatedAt,
                webhook.Description,
                webhook.CreatedBy
            );
        }

        public async Task DeleteWebhookAsync(string id)
        {
            var webhook = await _context.Webhooks.FindAsync(id);
            if (webhook == null) throw new ArgumentException("Webhook not found");

            _context.Webhooks.Remove(webhook);
            await _context.SaveChangesAsync();
        }

        public async Task<WebhookTestResponse> TestWebhookAsync(WebhookTestRequest request)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var payload = JsonSerializer.Serialize(request.Payload);
                var signature = GenerateSignature(payload, request.Secret);
                
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                content.Headers.Add("X-Webhook-Signature", signature);
                content.Headers.Add("X-Webhook-Event", request.EventType);
                content.Headers.Add("X-Webhook-Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

                var response = await _httpClient.PostAsync(request.Url, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                
                stopwatch.Stop();

                return new WebhookTestResponse(
                    response.IsSuccessStatusCode,
                    (int)response.StatusCode,
                    responseBody,
                    null,
                    stopwatch.Elapsed
                );
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new WebhookTestResponse(
                    false,
                    null,
                    null,
                    ex.Message,
                    stopwatch.Elapsed
                );
            }
        }

        public async Task<IEnumerable<WebhookDeliveryDto>> GetWebhookDeliveriesAsync(string webhookId, int page = 1, int pageSize = 50)
        {
            var deliveries = await _context.WebhookDeliveries
                .Where(d => d.WebhookId == webhookId)
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return deliveries.Select(d => new WebhookDeliveryDto(
                d.Id,
                d.WebhookId,
                d.EventType,
                d.Status,
                d.Attempts,
                d.ResponseCode,
                d.ResponseBody,
                d.ErrorMessage,
                d.CreatedAt,
                d.DeliveredAt,
                d.NextRetryAt
            ));
        }

        public async Task TriggerWebhookAsync(string eventType, object payload)
        {
            var activeWebhooks = await _context.Webhooks
                .Where(w => w.IsActive)
                .ToListAsync();

            foreach (var webhook in activeWebhooks)
            {
                try
                {
                    var events = JsonSerializer.Deserialize<string[]>(webhook.Events);
                    if (events?.Contains(eventType) == true)
                    {
                        await CreateDeliveryAsync(webhook.Id, eventType, payload);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing webhook {WebhookId} for event {EventType}", webhook.Id, eventType);
                }
            }
        }

        public async Task ProcessPendingDeliveriesAsync()
        {
            var pendingDeliveries = await _context.WebhookDeliveries
                .Where(d => d.Status == "pending" && (d.NextRetryAt == null || d.NextRetryAt <= DateTime.UtcNow))
                .Include(d => d.Webhook)
                .ToListAsync();

            foreach (var delivery in pendingDeliveries)
            {
                await ProcessDeliveryAsync(delivery);
            }
        }

        private async Task CreateDeliveryAsync(string webhookId, string eventType, object payload)
        {
            var delivery = new WebhookDelivery
            {
                WebhookId = webhookId,
                EventType = eventType,
                Payload = JsonSerializer.Serialize(payload),
                Status = "pending",
                NextRetryAt = DateTime.UtcNow
            };

            _context.WebhookDeliveries.Add(delivery);
            await _context.SaveChangesAsync();
        }

        private async Task ProcessDeliveryAsync(WebhookDelivery delivery)
        {
            try
            {
                delivery.Attempts++;
                
                var webhook = delivery.Webhook;
                var signature = GenerateSignature(delivery.Payload, webhook.Secret);
                
                var content = new StringContent(delivery.Payload, Encoding.UTF8, "application/json");
                content.Headers.Add("X-Webhook-Signature", signature);
                content.Headers.Add("X-Webhook-Event", delivery.EventType);
                content.Headers.Add("X-Webhook-Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(webhook.TimeoutSeconds));
                var response = await _httpClient.PostAsync(webhook.Url, content, cts.Token);
                var responseBody = await response.Content.ReadAsStringAsync();

                delivery.ResponseCode = (int)response.StatusCode;
                delivery.ResponseBody = responseBody;

                if (response.IsSuccessStatusCode)
                {
                    delivery.Status = "delivered";
                    delivery.DeliveredAt = DateTime.UtcNow;
                    delivery.NextRetryAt = null;
                }
                else
                {
                    delivery.ErrorMessage = $"HTTP {response.StatusCode}: {responseBody}";
                    await ScheduleRetryAsync(delivery, webhook);
                }
            }
            catch (Exception ex)
            {
                delivery.ErrorMessage = ex.Message;
                await ScheduleRetryAsync(delivery, delivery.Webhook);
            }
            finally
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task ScheduleRetryAsync(WebhookDelivery delivery, Webhook webhook)
        {
            if (delivery.Attempts < webhook.RetryCount)
            {
                delivery.Status = "pending";
                delivery.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, delivery.Attempts - 1)); // Exponential backoff
            }
            else
            {
                delivery.Status = "failed";
                delivery.NextRetryAt = null;
            }
        }

        private string GenerateSignature(string payload, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return "sha256=" + Convert.ToHexString(hash).ToLower();
        }
    }
}
