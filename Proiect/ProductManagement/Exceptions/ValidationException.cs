namespace ProductManagement.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : BaseException
{
    /// <summary>
    /// Gets the list of validation error messages.
    /// </summary>
    public List<string> Errors { get; } 
    
    /// <summary>
    /// Creates a validation exception from multiple error messages.
    /// </summary>
    /// <param name="errors">The collection of validation errors.</param>
    public ValidationException(IEnumerable<string> errors) : 
        base(string.Join("; ", errors), 400, "VALIDATION_ERROR")
    {
        Errors = errors.ToList();
    }

    /// <summary>
    /// Creates a validation exception from a single error message.
    /// </summary>
    /// <param name="error">The validation error message.</param>
    public ValidationException(string error) : base(error, 400, "VALIDATION_ERROR")
    {
        Errors = [error];
    }
    
}