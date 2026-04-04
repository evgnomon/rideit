using Microsoft.EntityFrameworkCore;
using rideit.Core.Data;
using rideit.Core.Models;
using rideit.Core.Services;
using rideit.Middleware;

var builder = WebApplication.CreateBuilder(args);

// EF Core — DbContext registration (skipped in Testing; tests provide their own)
var cosmosConnectionString = builder.Configuration.GetConnectionString("Cosmos");
if (builder.Environment.EnvironmentName == "Testing")
{
    // Tests register their own InMemory DbContext
}
else if (!string.IsNullOrEmpty(cosmosConnectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseCosmos(
            cosmosConnectionString,
            builder.Configuration["CosmosDatabase"] ?? "rideit",
            cosmosOptions =>
            {
                cosmosOptions.HttpClientFactory(() =>
                {
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    return new HttpClient(handler);
                });
            }));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("rideit"));
}

// Options pattern
builder.Services.Configure<WeatherOptions>(
    builder.Configuration.GetSection(WeatherOptions.SectionName));

// DI
builder.Services.AddScoped<IWeatherService, WeatherService>();

// Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddResponseCaching();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new() { Title = "RideIt API", Version = "v1" });
});

builder.Services.AddHttpClient("external", client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

if (app.Environment.EnvironmentName != "Testing")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();
app.UseCors("AllowAll");
app.UseResponseCaching();
app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/", () => new { message = "Welcome to RideIt API", version = "1.0" });
app.MapGet("/api/ping", () => Results.Ok(new { pong = DateTime.UtcNow }))
   .WithName("Ping").WithTags("Diagnostics");
app.MapPost("/api/echo", (object body) => Results.Ok(body))
   .WithTags("Diagnostics");
app.MapGet("/api/error", () =>
{
    throw new InvalidOperationException("Demo exception for global handler");
}).WithTags("Diagnostics");

app.Run();

public partial class Program { }
