using Microsoft.EntityFrameworkCore;
using rideit.Core.Models;

namespace rideit.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherForecast>().HasData(
            new WeatherForecast { Id = 1, Date = new DateTime(2026, 1, 1), TemperatureC = 22, Summary = "Warm" },
            new WeatherForecast { Id = 2, Date = new DateTime(2026, 1, 2), TemperatureC = 18, Summary = "Cool" },
            new WeatherForecast { Id = 3, Date = new DateTime(2026, 1, 3), TemperatureC = 30, Summary = "Hot" }
        );
    }
}
