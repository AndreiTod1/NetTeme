using Microsoft.EntityFrameworkCore;
using ProductManagement.Persistence;

namespace ProductManagement.Features.Products.ProductMetrics;

public class ProductMetricsHandler
{
    private readonly ApplicationContext _context;
    private readonly ILogger<ProductMetricsHandler> _logger;

    public ProductMetricsHandler(ApplicationContext context, ILogger<ProductMetricsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductMetricsDto> GetMetrics(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving product metrics");

        var products = await _context.Products.ToListAsync(cancellationToken);

        if (!products.Any())
        {
            return new ProductMetricsDto();
        }

        var metrics = new ProductMetricsDto
        {
            TotalProducts = products.Count,
            AveragePrice = products.Average(p => p.Price),
            TotalInventoryValue = products.Sum(p => p.Price * p.StockQuantity),
            TotalStockQuantity = products.Sum(p => p.StockQuantity),
            LowStockProducts = products.Count(p => p.StockQuantity > 0 && p.StockQuantity <= 5),
            OutOfStockProducts = products.Count(p => p.StockQuantity == 0)
        };

        metrics.ProductsByCategory = products
            .GroupBy(p => p.Category.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        metrics.AveragePriceByCategory = products
            .GroupBy(p => p.Category.ToString())
            .ToDictionary(g => g.Key, g => g.Average(p => p.Price));

        var mostExpensive = products.OrderByDescending(p => p.Price).First();
        metrics.MostExpensiveProduct = new ProductStatsDto
        {
            Name = mostExpensive.Name,
            Sku = mostExpensive.Sku,
            Price = mostExpensive.Price,
            Category = mostExpensive.Category.ToString()
        };

        var cheapest = products.OrderBy(p => p.Price).First();
        metrics.CheapestProduct = new ProductStatsDto
        {
            Name = cheapest.Name,
            Sku = cheapest.Sku,
            Price = cheapest.Price,
            Category = cheapest.Category.ToString()
        };

        metrics.CategoryBreakdown = products
            .GroupBy(p => p.Category)
            .Select(g => new CategoryStatsDto
            {
                Category = g.Key.ToString(),
                ProductCount = g.Count(),
                AveragePrice = g.Average(p => p.Price),
                TotalValue = g.Sum(p => p.Price * p.StockQuantity),
                TotalStock = g.Sum(p => p.StockQuantity)
            })
            .OrderByDescending(c => c.TotalValue)
            .ToList();

        _logger.LogInformation("Product metrics retrieved: {TotalProducts} products, {TotalValue:C} inventory value", 
            metrics.TotalProducts, metrics.TotalInventoryValue);

        return metrics;
    }
}
