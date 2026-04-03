using rideit.Core.Models;

namespace rideit.Core.Services;

public interface IWeatherService
{
    Task<IEnumerable<WeatherForecast>> GetAllAsync();
    Task<WeatherForecast?> GetByIdAsync(int id);
    Task<WeatherForecast> CreateAsync(CreateForecastRequest request);
    Task<WeatherForecast?> UpdateAsync(int id, UpdateForecastRequest request);
    Task<bool> DeleteAsync(int id);
}
