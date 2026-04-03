using Microsoft.AspNetCore.Mvc;
using rideit.Core.Models;
using rideit.Core.Services;
using rideit.Filters;

namespace rideit.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    [ResponseCache(Duration = 30)]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetAll()
    {
        var forecasts = await _weatherService.GetAllAsync();
        return Ok(forecasts);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WeatherForecast), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherForecast>> GetById(int id)
    {
        var forecast = await _weatherService.GetByIdAsync(id);
        if (forecast is null)
            return NotFound(new { message = $"Forecast {id} not found" });
        return Ok(forecast);
    }

    [HttpPost]
    [ValidateModel]
    [ProducesResponseType(typeof(WeatherForecast), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WeatherForecast>> Create([FromBody] CreateForecastRequest request)
    {
        var forecast = await _weatherService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = forecast.Id }, forecast);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WeatherForecast>> Update(int id, [FromBody] UpdateForecastRequest request)
    {
        var forecast = await _weatherService.UpdateAsync(id, request);
        if (forecast is null)
            return NotFound();
        return Ok(forecast);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _weatherService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> Search(
        [FromQuery] string? summary,
        [FromQuery] int? minTemp,
        [FromQuery] int? maxTemp)
    {
        var forecasts = await _weatherService.GetAllAsync();
        var filtered = forecasts
            .Where(f => summary == null || f.Summary.Contains(summary, StringComparison.OrdinalIgnoreCase))
            .Where(f => minTemp == null || f.TemperatureC >= minTemp)
            .Where(f => maxTemp == null || f.TemperatureC <= maxTemp);
        return Ok(filtered);
    }

    [HttpGet("header-demo")]
    public IActionResult HeaderDemo([FromHeader(Name = "X-Custom-Header")] string? customHeader)
    {
        return Ok(new { receivedHeader = customHeader ?? "(none)" });
    }
}
