namespace ProductManagement.Middleware;

/// <summary>
/// Represents a standardized error response for API exceptions.
/// </summary>
public class ErrorResponse()
{
    /// <summary>
    /// Creates an error response with code and message.
    /// </summary>
    /// <param name="errorCode">The error code identifier.</param>
    /// <param name="message">The error message.</param>
    public ErrorResponse(string errorCode, string message) : this()
    {
        ErrorCode = errorCode;
        Message = message;
        TraceId = string.Empty;
    }
    
    /// <summary>
    /// Creates an error response with code, message, and validation details.
    /// </summary>
    /// <param name="errorCode">The error code identifier.</param>
    /// <param name="message">The error message.</param>
    /// <param name="details">List of detailed error messages.</param>
    public ErrorResponse(string errorCode, string message, List<string> details) : this(errorCode, message)
    {
        Details = details;
    }

    /// <summary>
    /// Gets or sets the list of detailed error messages.
    /// </summary>
    public List<string> Details { get; set; } = new();

    /// <summary>
    /// Gets or sets the main error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the trace ID for tracking the request.
    /// </summary>
    public string TraceId { get; set; } = string.Empty;
}