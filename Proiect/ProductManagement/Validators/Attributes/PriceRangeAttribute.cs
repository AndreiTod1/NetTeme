using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Validators.Attributes;

/// <summary>
/// Validates that a price is within a specified range.
/// </summary>
public class PriceRangeAttribute : ValidationAttribute
{
    private readonly decimal _minPrice;
    private readonly decimal _maxPrice;

    /// <summary>
    /// Initializes a new instance with min and max price limits.
    /// </summary>
    /// <param name="minPrice">The minimum allowed price.</param>
    /// <param name="maxPrice">The maximum allowed price.</param>
    public PriceRangeAttribute(double minPrice, double maxPrice)
    {
        _minPrice = (decimal)minPrice;
        _maxPrice = (decimal)maxPrice;
        ErrorMessage = $"Price must be between {_minPrice:C2} and {_maxPrice:C2}.";
    }

    /// <summary>
    /// Validates that the price is within the specified range.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>Validation result indicating success or failure.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Price is required.");
        }

        if (value is decimal price)
        {
            if (price >= _minPrice && price <= _maxPrice)
            {
                return ValidationResult.Success;
            }
        }

        return new ValidationResult(ErrorMessage);
    }
}
