using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProductManagement.Validators.Attributes;

/// <summary>
/// Provides client-side SKU validation attributes.
/// Server-side validation is handled by FluentValidation.
/// </summary>
public class ValidSkuAttribute : ValidationAttribute, IClientModelValidator
{
    private const int MinLength = 5;
    private const int MaxLength = 20;
    private const string SkuPattern = @"^[a-zA-Z0-9-]{5,20}$";

    /// <summary>
    /// Initializes a new instance of the ValidSkuAttribute.
    /// </summary>
    public ValidSkuAttribute()
    {
        ErrorMessage = $"SKU must be alphanumeric with hyphens, {MinLength}-{MaxLength} characters.";
    }

    /// <summary>
    /// Validates the SKU format and length.
    /// Note: Detailed server-side validation is handled by FluentValidation.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>Validation result indicating success or failure.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Allow null/empty - FluentValidation will handle required validation
        if (value == null)
        {
            return ValidationResult.Success;
        }

        var sku = value.ToString();
        
        if (string.IsNullOrWhiteSpace(sku))
        {
            return ValidationResult.Success;
        }

        string cleanedSku = sku.Replace(" ", "");

        // Simple validation with generic message (consistent with client-side)
        if (!Regex.IsMatch(cleanedSku, SkuPattern))
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }

    /// <summary>
    /// Adds client-side validation attributes.
    /// </summary>
    /// <param name="context">The client validation context.</param>
    public void AddValidation(ClientModelValidationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-validsku", ErrorMessage ?? "Invalid SKU format.");
        MergeAttribute(context.Attributes, "data-val-validsku-pattern", SkuPattern);
        MergeAttribute(context.Attributes, "data-val-validsku-minlength", MinLength.ToString());
        MergeAttribute(context.Attributes, "data-val-validsku-maxlength", MaxLength.ToString());
    }

    /// <summary>
    /// Merges an attribute into the attributes dictionary if it doesn't already exist.
    /// </summary>
    /// <param name="attributes">The attributes dictionary.</param>
    /// <param name="key">The attribute key.</param>
    /// <param name="value">The attribute value.</param>
    /// <returns>True if the attribute was added, false if it already existed.</returns>
    private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (!attributes.ContainsKey(key))
        {
            attributes.Add(key, value);
        }
    }
}
