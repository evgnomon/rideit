using System.Diagnostics;

namespace rideit.Middleware;

// 6. Custom Middleware
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        context.Response.OnStarting(() =>
        {
            sw.Stop();
            context.Response.Headers["X-Response-Time-Ms"] = sw.ElapsedMilliseconds.ToString();
            return Task.CompletedTask;
        });
        await _next(context);
        _logger.LogInformation("{Method} {Path} completed in {Elapsed}ms",
            context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds);
    }
}
