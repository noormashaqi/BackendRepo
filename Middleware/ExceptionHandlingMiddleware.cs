using System.Net;
using System.Text.Json;
using FluentValidation;

namespace SupermarketSystem.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var errors = ex.Errors.Select(e => new
            {
                field = e.PropertyName,
                message = e.ErrorMessage
            });

            var result = JsonSerializer.Serialize(new
            {
                status = 400,
                message = "Validation failed",
                errors
            });

            await context.Response.WriteAsync(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            var result = JsonSerializer.Serialize(new
            {
                status = 401,
                message = ex.Message
            });

            await context.Response.WriteAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var result = JsonSerializer.Serialize(new
            {
                status = 500,
                message = "An unexpected error occurred"
            });

            await context.Response.WriteAsync(result);
        }
    }
}