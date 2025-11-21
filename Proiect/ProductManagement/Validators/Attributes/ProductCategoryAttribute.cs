using System.ComponentModel.DataAnnotations;
using ProductManagement.Features.Products;

namespace ProductManagement.Validators.Attributes;

/// <summary>
/// Validates that a product category is within the allowed values.
/// </summary>
public class ProductCategoryAttribute : ValidationAttribute
{
    private readonly ProductCategory[] _allowedCategories;

    /// <summary>
    /// Initializes a new instance with allowed categories.
    /// </summary>
    /// <param name="allowedCategories">The list of allowed categories.</param>
    public ProductCategoryAttribute(params ProductCategory[] allowedCategories)
    {
        _allowedCategories = allowedCategories;
        ErrorMessage = $"Category must be one of: {string.Join(", ", allowedCategories)}.";
    }

    /// <summary>
    /// Validates that the category is in the allowed list.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>Validation result indicating success or failure.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Category is required.");
        }

        if (value is ProductCategory category)
        {
            if (_allowedCategories.Contains(category))
            {
                return ValidationResult.Success;
            }
        }

        return new ValidationResult(ErrorMessage);
    }
}
