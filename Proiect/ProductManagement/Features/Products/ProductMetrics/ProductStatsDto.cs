namespace ProductManagement.Features.Products.ProductMetrics;

public class ProductStatsDto
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}

