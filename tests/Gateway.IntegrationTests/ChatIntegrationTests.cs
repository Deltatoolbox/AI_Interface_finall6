using FluentAssertions;
using Gateway.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Gateway.Infrastructure.Data;
using Gateway.Application.DTOs;
using Gateway.Domain.Interfaces;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace Gateway.IntegrationTests;

public class ChatIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ChatIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Set known admin password to bypass random generation in Program.cs
            builder.UseSetting("ADMIN_PASSWORD", "testpassword");

            builder.ConfigureServices(services =>
            {
                // Replace DbContext with InMemory
                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<GatewayDbContext>));
                if (dbDescriptor != null) services.Remove(dbDescriptor);
                services.AddDbContext<GatewayDbContext>(options => options.UseInMemoryDatabase("ChatTestDb"));

                // Replace LmStudioClient with Fake
                var lmDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ILmStudioClient));
                if (lmDescriptor != null) services.Remove(lmDescriptor);
                services.AddSingleton<ILmStudioClient, FakeLmStudioClient>();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Chat_ShouldPersistMessagesLinkedToConversation()
    {
        // 1. Login to get cookie
        var loginRequest = new LoginRequest("admin", "testpassword");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Send Chat Request
        var chatRequest = new ChatRequest(
            "test-model",
            new[] { new ChatMessageDto("user", "Hello AI") },
            0.7, 100, 1.0);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/chat");
        requestMessage.Content = JsonContent.Create(chatRequest);

        var chatResponse = await _client.SendAsync(requestMessage);
        chatResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Read stream to ensure processing completes
        var streamContent = await chatResponse.Content.ReadAsStringAsync();

        // 4. Verify Database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();

        var conversation = await dbContext.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync();

        conversation.Should().NotBeNull();
        conversation!.Title.Should().Be("Hello AI");
        conversation.Messages.Should().HaveCount(2); // User + Assistant

        var userMsg = conversation.Messages.First(m => m.Role == "user");
        userMsg.Content.Should().Be("Hello AI");
        userMsg.ConversationId.Should().Be(conversation.Id);

        var assistantMsg = conversation.Messages.First(m => m.Role == "assistant");
        // The mock returns data: ... so we expect that content
        assistantMsg.Content.Should().Contain("Dummy response");
        assistantMsg.ConversationId.Should().Be(conversation.Id);
    }
}

public class FakeLmStudioClient : ILmStudioClient
{
    public Task<IEnumerable<LmModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<LmModel>>(new[]
        {
            new LmModel("test-model", "model", 0, "openai")
        });
    }

    public Task<Stream> ChatCompletionStreamAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        var content = "data: {\"choices\":[{\"delta\":{\"content\":\"Dummy response\"}}]}\n\n";
        return Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(content)));
    }
}
