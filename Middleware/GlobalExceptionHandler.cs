using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineParkingLotSystem.Domain.Exceptions;

namespace OnlineParkingLotSystem.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;//shows the error msg
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred.");

        var (statusCode, title) = MapException(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception) =>
        exception switch
        {
            TicketNotFoundException => (StatusCodes.Status404NotFound, "Ticket not found"),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Authentication failed"),
            VehicleAlreadyParkedException => (StatusCodes.Status409Conflict, "Vehicle already parked"),
            VehicleTypeMismatchException => (StatusCodes.Status400BadRequest, "Vehicle type mismatch"),
            TicketAlreadyClosedException => (StatusCodes.Status409Conflict, "Ticket already closed"),
            NoAvailableSpotException => (StatusCodes.Status409Conflict, "No available spot"),
            SpotAlreadyOccupiedException => (StatusCodes.Status409Conflict, "Spot already occupied"),
            SpotAlreadyFreeException => (StatusCodes.Status409Conflict, "Spot already free"),
            DomainException => (StatusCodes.Status400BadRequest, "Domain validation failed"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
}
