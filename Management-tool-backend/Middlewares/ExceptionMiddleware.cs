using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using RecamNewBackend.Common;
using RecamNewBackend.Exceptions;

namespace RecamNewBackend.Middlewares;

public class ExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException validationEx => ApiResponse<object>.Fail(validationEx.Message, "VALIDATION_ERROR"),
            NotFoundException notFoundEx => ApiResponse<object>.Fail(notFoundEx.Message, "NOT_FOUND"),
            UnauthorizedAccessException unauthorizedEx => ApiResponse<object>.Fail(unauthorizedEx.Message, "UNAUTHORIZED"),
            _ => ApiResponse<object>.Fail("An error occurred while processing your request", "INTERNAL_ERROR")
        };

        context.Response.StatusCode = GetStatusCode(exception);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        ValidationException => 400,
        NotFoundException => 404,
        UnauthorizedAccessException => 401,
        _ => 500
    };
}
