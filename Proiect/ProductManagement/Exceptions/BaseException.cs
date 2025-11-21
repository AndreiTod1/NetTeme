namespace ProductManagement.Exceptions;

/// <summary>
/// Base class for all custom application exceptions.
/// </summary>
public class BaseException : Exception
{
    public int StatusCode { get;}

    public string ErrorCode { get;}
    
    /// <summary>
    /// Creates a new base exception with message, status code, and error code.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="errorCode">The error code identifier.</param>
    protected BaseException(string message, int statusCode, string errorCode) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
    
    /// <summary>
    /// Creates a new base exception with message, inner exception, status code, and error code.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="errorCode">The error code identifier.</param>
    protected BaseException(string message, Exception innerException, int statusCode, string errorCode) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}