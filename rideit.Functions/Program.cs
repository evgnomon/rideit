using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using rideit.Core.Data;
using rideit.Core.Models;
using rideit.Core.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        var cosmosConnection = Environment.GetEnvironmentVariable("ConnectionStrings__Cosmos");
        if (!string.IsNullOrEmpty(cosmosConnection))
        {
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseCosmos(
                    cosmosConnection,
                    Environment.GetEnvironmentVariable("CosmosDatabase") ?? "rideit"));
        }
        else
        {
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase("rideit"));
        }

        services.Configure<WeatherOptions>(opts =>
        {
            opts.DefaultSummary = "Mild";
            opts.MaxForecastDays = 5;
        });

        services.AddScoped<IWeatherService, WeatherService>();
    })
    .Build();

// Auto-create DB (best-effort; logs warning if Cosmos is unreachable)
try
{
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Warning: Database initialization failed: {ex.Message}");
}

await host.RunAsync();
