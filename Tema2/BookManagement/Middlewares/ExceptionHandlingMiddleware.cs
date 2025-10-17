using System.Net;
using BookManagement.Exceptions;

namespace BookManagement.Middlewares;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message;

        switch (exception)
        {
            case BookNotFoundException notFoundEx:
                statusCode = (int)HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                break;

            case InvalidSortException sortEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = sortEx.Message;
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred.";
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = new
        {
            StatusCode = statusCode,
            Message = message
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}