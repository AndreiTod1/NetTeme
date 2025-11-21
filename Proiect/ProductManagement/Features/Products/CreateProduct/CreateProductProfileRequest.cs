using ProductManagement.Validators.Attributes;

namespace ProductManagement.Features.Products.CreateProduct;

/// <summary>
/// Represents the request to create a new product.
/// </summary>
public class CreateProductProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    
    [ValidSku]
    public string Sku { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public decimal Price { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; } = 1;
}
