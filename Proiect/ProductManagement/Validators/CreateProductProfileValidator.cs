using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductManagement.Common.Logging;
using ProductManagement.Features.Products;
using ProductManagement.Features.Products.CreateProduct;
using ProductManagement.Persistence;

namespace ProductManagement.Validators;

/// <summary>
/// Validator for product creation requests with business rules and constraints.
/// </summary>
public class CreateProductProfileValidator : AbstractValidator<CreateProductProfileRequest>
{
    private readonly ApplicationContext _context;
    private readonly ILogger<CreateProductProfileValidator> _logger;
    
    private static readonly string[] InappropriateWords = { "inappropriate", "offensive", "bad" };
    private static readonly string[] RestrictedWordsForHome = { "weapon", "dangerous", "hazardous" };
    private static readonly string[] TechnologyKeywords = { "tech", "smart", "digital", "electronic", "wireless", "bluetooth", "wifi", "computer", "phone", "tablet", "laptop" };
    
    public CreateProductProfileValidator(ApplicationContext context, ILogger<CreateProductProfileValidator> logger)
    {
        _context = context;
        _logger = logger;
        
        // Name Validation Rules
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name must not be empty.")
            .Length(1, 200).WithMessage("Product name must be between 1 and 200 characters.")
            .Must(BeValidName).WithMessage("Product name contains inappropriate content.")
            .MustAsync(BeUniqueName).WithMessage("A product with this name already exists for this brand.");

        // Brand Validation Rules
        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand must not be empty.")
            .Length(2, 100).WithMessage("Brand must be between 2 and 100 characters.")
            .Must(BeValidBrandName).WithMessage("Brand name contains invalid characters. Only letters, spaces, hyphens, apostrophes, dots, and numbers are allowed.");

        // SKU Validation Rules
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU must not be empty.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Sku)
                    .Must(BeValidSku).WithMessage("SKU must be alphanumeric with hyphens, 5-20 characters.")
                    .MustAsync(BeUniqueSku).WithMessage("SKU already exists in the system.");
            });

        // Category Validation Rules
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid enum value.");

        // Price Validation Rules
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThan(10000).WithMessage("Price must be less than $10,000.");

        // ReleaseDate Validation Rules
        RuleFor(x => x.ReleaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Release date cannot be in the future.")
            .GreaterThan(new DateTime(1900, 1, 1)).WithMessage("Release date cannot be before year 1900.");

        // StockQuantity Validation Rules
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
            .LessThanOrEqualTo(100000).WithMessage("Stock quantity cannot exceed 100,000.");

        // ImageUrl Validation Rules
        RuleFor(x => x.ImageUrl)
            .Must(BeValidImageUrl).WithMessage("Image URL must be a valid HTTP/HTTPS URL ending with .jpg, .jpeg, .png, .gif, or .webp.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
        
        // Business Rules Validation
        RuleFor(x => x)
            .MustAsync(PassBusinessRules).WithMessage("Product does not meet business rule requirements.");
        
        // Conditional Validation: Electronics
        When(x => x.Category == ProductCategory.Electronics, () =>
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(50.00m).WithMessage("Electronics products must have a minimum price of $50.00.");
            
            RuleFor(x => x.Name)
                .Must(ContainTechnologyKeywords).WithMessage("Electronics products must contain technology-related keywords in the name.");
            
            RuleFor(x => x)
                .Must(x => (DateTime.UtcNow - x.ReleaseDate).TotalDays <= 1825).WithMessage("Electronics products must be released within the last 5 years.");
        });
        
        // Conditional Validation: Home
        When(x => x.Category == ProductCategory.Home, () =>
        {
            RuleFor(x => x.Price)
                .LessThanOrEqualTo(200.00m).WithMessage("Home products cannot exceed $200.00.");
            
            RuleFor(x => x.Name)
                .Must(BeAppropriateForHome).WithMessage("Home product name contains restricted words.");
        });
        
        // Conditional Validation: Clothing
        When(x => x.Category == ProductCategory.Clothing, () =>
        {
            RuleFor(x => x.Brand)
                .MinimumLength(3).WithMessage("Clothing products must have a brand name of at least 3 characters.");
        });
        
        // Cross-Field Validation
        RuleFor(x => x)
            .Must(x => x.Price <= 100 || x.StockQuantity <= 20)
            .WithMessage("Expensive products (>$100) must have limited stock (≤20 units).");
    }

    private bool BeValidName(string name)
    {
        return !InappropriateWords.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> BeUniqueName(CreateProductProfileRequest request, string name, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking name uniqueness for Name={Name}, Brand={Brand}", name, request.Brand);
        
        var exists = await _context.Products
            .AnyAsync(p => p.Name == name && p.Brand == request.Brand, cancellationToken);
        
        return !exists;
    }

    private bool BeValidBrandName(string brand)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(brand, @"^[a-zA-Z0-9\s\-'.]+$");
    }

    private bool BeValidSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;
            
        var cleanedSku = sku.Replace(" ", "");
        return System.Text.RegularExpressions.Regex.IsMatch(cleanedSku, @"^[a-zA-Z0-9-]{5,20}$");
    }

    private async Task<bool> BeUniqueSku(string sku, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return true; 
        
        _logger.LogInformation(ProductLogEvents.SKUValidationPerformed, "Checking SKU uniqueness for SKU={SKU}", sku);
        
        var exists = await _context.Products.AnyAsync(p => p.Sku == sku, cancellationToken);
        
        return !exists;
    }

    private bool BeValidImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return true;
        
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            return false;
        
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;
        
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        return validExtensions.Any(ext => imageUrl.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> PassBusinessRules(CreateProductProfileRequest request, CancellationToken cancellationToken)
    {
        // Rule 1: Daily product addition limit check (max 500 per day)
        var today = DateTime.UtcNow.Date;
        var todayCount = await _context.Products
            .CountAsync(p => p.CreatedAt >= today, cancellationToken);
        
        if (todayCount >= 500)
        {
            _logger.LogWarning("Daily product addition limit reached: {Count}/500", todayCount);
            return false;
        }
        
        // Rule 2: Electronics minimum price check ($50.00)
        if (request.Category == ProductCategory.Electronics && request.Price < 50.00m)
        {
            _logger.LogWarning("Electronics product below minimum price: ${Price}", request.Price);
            return false;
        }
        
        // Rule 3: Home product content restrictions
        if (request.Category == ProductCategory.Home)
        {
            if (RestrictedWordsForHome.Any(word => request.Name.Contains(word, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Home product contains restricted words in name: {Name}", request.Name);
                return false;
            }
        }
        
        // Rule 4: High-value product stock limit (>$500 = max 10 stock)
        if (request.Price > 500 && request.StockQuantity > 10)
        {
            _logger.LogWarning("High-value product (${Price}) exceeds stock limit: {Stock} > 10", request.Price, request.StockQuantity);
            return false;
        }
        
        return true;
    }

    private bool ContainTechnologyKeywords(string name)
    {
        return TechnologyKeywords.Any(keyword => name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private bool BeAppropriateForHome(string name)
    {
        return !RestrictedWordsForHome.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase));
    }
}