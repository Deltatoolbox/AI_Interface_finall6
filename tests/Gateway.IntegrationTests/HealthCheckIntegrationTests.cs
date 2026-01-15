using FluentAssertions;
using Gateway.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Gateway.Infrastructure.Data;
using Xunit;

namespace Gateway.IntegrationTests;

public class HealthCheckIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthCheckIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<GatewayDbContext>));
                
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<GatewayDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthLive_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health/live");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthReady_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health/ready");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }
}
