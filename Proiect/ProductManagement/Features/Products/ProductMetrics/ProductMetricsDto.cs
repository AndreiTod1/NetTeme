namespace ProductManagement.Features.Products.ProductMetrics;

public class ProductMetricsDto
{
    public int TotalProducts { get; set; }
    public Dictionary<string, int> ProductsByCategory { get; set; } = new();
    public decimal AveragePrice { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int TotalStockQuantity { get; set; }
    public Dictionary<string, decimal> AveragePriceByCategory { get; set; } = new();
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public ProductStatsDto MostExpensiveProduct { get; set; } = new();
    public ProductStatsDto CheapestProduct { get; set; } = new();
    public List<CategoryStatsDto> CategoryBreakdown { get; set; } = new();
}

