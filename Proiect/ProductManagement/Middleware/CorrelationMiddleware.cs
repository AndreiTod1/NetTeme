using Microsoft.Extensions.Primitives;

namespace ProductManagement.Middleware;

/// <summary>
/// Middleware that adds correlation IDs to requests for tracking across services.
/// </summary>
public class CorrelationMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationMiddleware> _logger;

    public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Processes the HTTP request and adds correlation ID tracking.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Add correlation ID to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);
            return Task.CompletedTask;
        });

        // Add correlation ID to logging scope
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            _logger.LogInformation("Request started: {Method} {Path} - CorrelationId: {CorrelationId}", 
                context.Request.Method, 
                context.Request.Path, 
                correlationId);

            await _next(context);

            _logger.LogInformation("Request completed: {Method} {Path} - Status: {StatusCode} - CorrelationId: {CorrelationId}", 
                context.Request.Method, 
                context.Request.Path, 
                context.Response.StatusCode,
                correlationId);
        }
    }

    /// <summary>
    /// Gets the correlation ID from request headers or creates a new one.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>The correlation ID for this request.</returns>
    private string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
