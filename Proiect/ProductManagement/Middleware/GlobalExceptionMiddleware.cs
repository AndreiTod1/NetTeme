using System.Net;
using System.Text.Json;
using ProductManagement.Exceptions;

namespace ProductManagement.Middleware;

/// <summary>
/// Middleware that catches and handles all unhandled exceptions globally.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionMiddleware> logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }
    
    /// <summary>
    /// Processes the HTTP request and catches any exceptions.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles exceptions and formats them as JSON error responses.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="exception">The exception that was thrown.</param>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        ErrorResponse errorResponse;
        int statusCode;

        switch (exception)
        {
            case ValidationException validationEx:
                errorResponse = new ErrorResponse(validationEx.ErrorCode, validationEx.Message, validationEx.Errors)
                {
                    TraceId = context.TraceIdentifier
                };
                statusCode = validationEx.StatusCode;
                break;
                
            case BaseException baseEx:
                errorResponse = new ErrorResponse(baseEx.ErrorCode, baseEx.Message)
                {
                    TraceId = context.TraceIdentifier
                };
                statusCode = baseEx.StatusCode;
                break;
                
            default:
                errorResponse = new ErrorResponse("INTERNAL_SERVER_ERROR", "An unexpected error occurred")
                {
                    TraceId = context.TraceIdentifier
                };
                statusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        context.Response.StatusCode = statusCode;
        var response = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(response);
    }
}