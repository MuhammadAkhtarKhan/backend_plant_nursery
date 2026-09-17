using System.Text.Json;
using PlantNursery.Application.Common;

namespace PlantNursery.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception occurred. Path: {Path}",
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,

            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,

            KeyNotFoundException => StatusCodes.Status404NotFound,

            InvalidOperationException => StatusCodes.Status409Conflict,

            _ =>  StatusCodes.Status500InternalServerError
        };

        var message = exception switch
        {
            ArgumentException => exception.Message,

            UnauthorizedAccessException => exception.Message,

            KeyNotFoundException => exception.Message,

            InvalidOperationException =>  exception.Message,

            _ => "An unexpected error occurred."
        };

        var response = ApiResponse.Fail(message);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync( JsonSerializer.Serialize(response));
    }
}