using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using rideit.Core.Data;

namespace rideit.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Seed the in-memory database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}

public class WeatherControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public WeatherControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithForecasts()
    {
        var response = await _client.GetAsync("/api/weather");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("temperatureC", content);
        Assert.Contains("summary", content);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        var response = await _client.GetAsync("/api/weather/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var response = await _client.GetAsync("/api/weather/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WithValidRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/weather", new
        {
            TemperatureC = 25,
            Summary = "Sunny"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Sunny", content);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WithInvalidTemp()
    {
        var response = await _client.PostAsJsonAsync("/api/weather", new
        {
            TemperatureC = 100,
            Summary = "Invalid"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenExists()
    {
        var response = await _client.PutAsJsonAsync("/api/weather/1", new
        {
            Summary = "Updated"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Updated", content);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenExists()
    {
        // Create one to delete
        var createResponse = await _client.PostAsJsonAsync("/api/weather", new
        {
            TemperatureC = 10,
            Summary = "ToDelete"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        var response = await _client.DeleteAsync($"/api/weather/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Search_FiltersResults()
    {
        var response = await _client.GetAsync("/api/weather/search?summary=Warm");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MinimalApi_Root_ReturnsWelcome()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Welcome to RideIt API", content);
    }

    [Fact]
    public async Task MinimalApi_Ping_ReturnsPong()
    {
        var response = await _client.GetAsync("/api/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("pong", content);
    }

    [Fact]
    public async Task ErrorEndpoint_ReturnsInternalServerError()
    {
        var response = await _client.GetAsync("/api/error");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error", content);
    }

    [Fact]
    public async Task ResponseContainsTimingHeader()
    {
        var response = await _client.GetAsync("/api/weather");

        Assert.True(response.Headers.Contains("X-Response-Time-Ms"));
    }

    [Fact]
    public async Task HeaderDemo_ReturnsCustomHeader()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/weather/header-demo");
        request.Headers.Add("X-Custom-Header", "test-value");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("test-value", content);
    }
}
