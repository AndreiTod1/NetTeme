namespace ProductManagement.Features.Products.ProductMetrics;

public class CategoryStatsDto
{
    public string Category { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TotalValue { get; set; }
    public int TotalStock { get; set; }
}

