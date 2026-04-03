using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using rideit.Core.Data;
using rideit.Core.Models;

namespace rideit.Core.Services;

public class WeatherService : IWeatherService
{
    private readonly AppDbContext _db;
    private readonly ILogger<WeatherService> _logger;
    private readonly WeatherOptions _options;

    public WeatherService(AppDbContext db, ILogger<WeatherService> logger, IOptions<WeatherOptions> options)
    {
        _db = db;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<IEnumerable<WeatherForecast>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all forecasts");
        return await _db.WeatherForecasts.ToListAsync();
    }

    public async Task<WeatherForecast?> GetByIdAsync(int id)
    {
        return await _db.WeatherForecasts.FindAsync(id);
    }

    public async Task<WeatherForecast> CreateAsync(CreateForecastRequest request)
    {
        var forecast = new WeatherForecast
        {
            Date = request.Date ?? DateTime.Now,
            TemperatureC = request.TemperatureC,
            Summary = request.Summary ?? _options.DefaultSummary
        };
        _db.WeatherForecasts.Add(forecast);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created forecast {Id}", forecast.Id);
        return forecast;
    }

    public async Task<WeatherForecast?> UpdateAsync(int id, UpdateForecastRequest request)
    {
        var existing = await _db.WeatherForecasts.FindAsync(id);
        if (existing is null) return null;

        if (request.TemperatureC.HasValue)
            existing.TemperatureC = request.TemperatureC.Value;
        if (request.Summary is not null)
            existing.Summary = request.Summary;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var forecast = await _db.WeatherForecasts.FindAsync(id);
        if (forecast is null) return false;

        _db.WeatherForecasts.Remove(forecast);
        await _db.SaveChangesAsync();
        return true;
    }
}
