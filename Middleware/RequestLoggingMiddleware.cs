using System.Diagnostics;

namespace OnlineParkingLotSystem.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        _logger.LogInformation(
            "Incoming request {Method} {Path}",
            request.Method,
            request.Path);

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "Completed request {Method} {Path} with status {StatusCode} in {ElapsedMs}ms",
            request.Method,
            request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
