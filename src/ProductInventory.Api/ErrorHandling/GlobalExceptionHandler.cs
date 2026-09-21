using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductInventory.Domain.Exceptions;

namespace ProductInventory.Api.ErrorHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);
        var isUnhandled = statusCode == StatusCodes.Status500InternalServerError;

        if (isUnhandled)
        {
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = isUnhandled ? "An unexpected error occurred. Please try again later." : exception.Message,
            Instance = httpContext.Request.Path,
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        ProductNotFoundException => (StatusCodes.Status404NotFound, "Product not found"),
        InsufficientStockException => (StatusCodes.Status409Conflict, "Insufficient stock"),
        ProductConcurrencyConflictException => (StatusCodes.Status409Conflict, "Concurrency conflict"),
        FormatException => (StatusCodes.Status400BadRequest, "Malformed request"),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred"),
    };
}
