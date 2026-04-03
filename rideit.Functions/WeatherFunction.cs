using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using rideit.Core.Models;
using rideit.Core.Services;

namespace rideit.Functions;

public class WeatherFunction
{
    private readonly IWeatherService _service;

    public WeatherFunction(IWeatherService service)
    {
        _service = service;
    }

    [Function("GetAllForecasts")]
    public async Task<HttpResponseData> GetAll(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather")] HttpRequestData req)
    {
        var forecasts = await _service.GetAllAsync();
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(forecasts);
        return response;
    }

    [Function("GetForecastById")]
    public async Task<HttpResponseData> GetById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather/{id:int}")] HttpRequestData req,
        int id)
    {
        var forecast = await _service.GetByIdAsync(id);
        if (forecast is null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteAsJsonAsync(new { message = $"Forecast {id} not found" });
            return notFound;
        }
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(forecast);
        return response;
    }

    [Function("CreateForecast")]
    public async Task<HttpResponseData> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "weather")] HttpRequestData req)
    {
        var request = await req.ReadFromJsonAsync<CreateForecastRequest>();
        if (request is null)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteAsJsonAsync(new { message = "Invalid request body" });
            return bad;
        }
        var forecast = await _service.CreateAsync(request);
        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(forecast);
        return response;
    }

    [Function("UpdateForecast")]
    public async Task<HttpResponseData> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "weather/{id:int}")] HttpRequestData req,
        int id)
    {
        var request = await req.ReadFromJsonAsync<UpdateForecastRequest>();
        if (request is null)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteAsJsonAsync(new { message = "Invalid request body" });
            return bad;
        }
        var forecast = await _service.UpdateAsync(id, request);
        if (forecast is null)
            return req.CreateResponse(HttpStatusCode.NotFound);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(forecast);
        return response;
    }

    [Function("DeleteForecast")]
    public async Task<HttpResponseData> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "weather/{id:int}")] HttpRequestData req,
        int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return req.CreateResponse(deleted ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
    }
}
