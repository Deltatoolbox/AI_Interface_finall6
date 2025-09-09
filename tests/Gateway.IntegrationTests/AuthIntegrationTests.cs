using FluentAssertions;
using Gateway.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Json;
using Gateway.Infrastructure.Data;
using Gateway.Application.DTOs;

namespace Gateway.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests(WebApplicationFactory<Program> factory)
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
    public async Task Login_ValidCredentials_ShouldReturnOk()
    {
        var loginRequest = new LoginRequest("admin", "admin");
        
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShouldReturnUnauthorized()
    {
        var loginRequest = new LoginRequest("admin", "wrongpassword");
        
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCsrfToken_ShouldReturnToken()
    {
        var response = await _client.GetAsync("/api/auth/csrf");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("csrfToken");
    }

    [Fact]
    public async Task Logout_ShouldReturnNoContent()
    {
        var response = await _client.PostAsync("/api/auth/logout", null);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
