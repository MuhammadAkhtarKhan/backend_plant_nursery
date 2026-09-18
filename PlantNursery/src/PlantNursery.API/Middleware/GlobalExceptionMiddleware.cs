using System.Text.Json;
using PlantNursery.Application.Common;
using PlantNursery.Application.Common.Exceptions;

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

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,

            UnauthorizedException => StatusCodes.Status401Unauthorized,

            ForbiddenException => StatusCodes.Status403Forbidden,

            NotFoundException => StatusCodes.Status404NotFound,

            ConflictException => StatusCodes.Status409Conflict,

            _ => StatusCodes.Status500InternalServerError
        };

        var message = exception switch
        {
            ValidationException => exception.Message,

            UnauthorizedException => exception.Message,

            ForbiddenException => exception.Message,

            NotFoundException => exception.Message,

            ConflictException => exception.Message,

            _ => "An unexpected error occurred."
        };

        var response = ApiResponse.Fail(message);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}