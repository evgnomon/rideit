using rideit.Core.Models;

namespace rideit.Core.Services;

public interface IWeatherService
{
    Task<IEnumerable<WeatherForecast>> GetAllAsync();
    Task<WeatherForecast?> GetByIdAsync(string id);
    Task<WeatherForecast> CreateAsync(CreateForecastRequest request);
    Task<WeatherForecast?> UpdateAsync(string id, UpdateForecastRequest request);
    Task<bool> DeleteAsync(string id);
}
