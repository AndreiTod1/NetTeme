using System.Data.Common;
using FluentValidation;
using Tema3.Features.Products;

namespace Tema3.Validators;
public class CreateProductProfileValidator : AbstractValidator<CreateProductProfileRequest>
{
    private readonly ApplicationContext _context;
    private readonly ILogger<CreateProductProfileValidator> _logger;
    
    
    public CreateProductProfileValidator(ApplicationContext context, ILogger<CreateProductProfileValidator> logger)
    {
        _context = context;
        _logger = logger;
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name must not be empty.")
            .Length(1, 200).WithMessage("Product name must be between 1 and 200 characters.");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand must not be empty.")
            .Length(2, 100).WithMessage("Brand must be between 2 and 100 characters.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU must not be empty.")
            .Matches(@"^[a-zA-Z0-9-]{5,20}$").WithMessage("SKU must be alphanumeric with hyphens, 5-20 characters.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid enum value.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThan(10000).WithMessage("Price must be less than $10,000.");

        RuleFor(x => x.ReleaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Release date cannot be in the future.")
            .GreaterThan(new DateTime(1900, 1, 1)).WithMessage("Release date cannot be before year 1900.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
            .LessThanOrEqualTo(100000).WithMessage("Stock quantity cannot exceed 100,000.");

        RuleFor(x => x.ImageUrl)
            .Must(url => string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage("Image URL must be valid when provided.")
            .Must(url => string.IsNullOrEmpty(url) || (url.StartsWith("http://") || url.StartsWith("https://")))
                .WithMessage("Image URL must use HTTP/HTTPS protocol.")
            .Must(url => string.IsNullOrEmpty(url) || url.EndsWith(".jpg") || url.EndsWith(".jpeg") ||
                         url.EndsWith(".png") || url.EndsWith(".gif") || url.EndsWith(".webp"))
                .WithMessage("Image URL must end with a valid image extension (.jpg, .jpeg, .png, .gif, .webp).");
        
        
    }
}