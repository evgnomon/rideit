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
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlite(Environment.GetEnvironmentVariable("DbConnection") ?? "Data Source=rideit.db"));

        services.Configure<WeatherOptions>(opts =>
        {
            opts.DefaultSummary = "Mild";
            opts.MaxForecastDays = 5;
        });

        services.AddScoped<IWeatherService, WeatherService>();
    })
    .Build();

// Auto-create DB
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

host.Run();
