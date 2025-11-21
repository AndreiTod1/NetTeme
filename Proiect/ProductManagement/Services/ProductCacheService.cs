using Microsoft.Extensions.Caching.Memory;
using ProductManagement.Features.Products;

namespace ProductManagement.Services;

/// <summary>
/// Service for managing product cache operations with memory cache.
/// </summary>
public class ProductCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductCacheService> _logger;
    private const string CacheKeyPrefix = "products";
    private const int DefaultCacheMinutes = 10;

    public ProductCacheService(IMemoryCache cache, ILogger<ProductCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Gets the cache key for a specific product category.
    /// </summary>
    /// <param name="category">The product category.</param>
    /// <returns>The cache key string for the category.</returns>
    public string GetCategoryKey(ProductCategory category)
    {
        return $"{CacheKeyPrefix}:category:{category}";
    }

    /// <summary>
    /// Gets the cache key for all products.
    /// </summary>
    /// <returns>The cache key string for all products.</returns>
    public string GetAllProductsKey()
    {
        return $"{CacheKeyPrefix}:all";
    }

    /// <summary>
    /// Stores products in cache for a specific category.
    /// </summary>
    /// <param name="category">The product category.</param>
    /// <param name="products">The products to cache.</param>
    public void CacheProductsByCategory(ProductCategory category, IEnumerable<Product> products)
    {
        var key = GetCategoryKey(category);
        var productList = products.ToList();
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(DefaultCacheMinutes))
            .SetPriority(CacheItemPriority.Normal);

        _cache.Set(key, productList, cacheOptions);
        _logger.LogInformation("Cached {Count} products for category {Category} with key {Key}", 
            productList.Count, category, key);
    }

    /// <summary>
    /// Retrieves cached products for a specific category.
    /// </summary>
    /// <param name="category">The product category.</param>
    /// <returns>The cached products, or null if not found.</returns>
    public List<Product>? GetProductsByCategory(ProductCategory category)
    {
        var key = GetCategoryKey(category);
        if (_cache.TryGetValue(key, out List<Product>? products))
        {
            _logger.LogInformation("Cache hit for category {Category}", category);
            return products;
        }

        _logger.LogInformation("Cache miss for category {Category}", category);
        return null;
    }

    /// <summary>
    /// Removes cached data for a specific category.
    /// </summary>
    /// <param name="category">The product category to invalidate.</param>
    public void InvalidateCategoryCache(ProductCategory category)
    {
        var key = GetCategoryKey(category);
        _cache.Remove(key);
        _logger.LogInformation("Invalidated cache for category {Category}", category);
    }

    /// <summary>
    /// Removes all cached product data.
    /// </summary>
    public void InvalidateAllCaches()
    {
        // Invalidate all category caches
        foreach (ProductCategory category in Enum.GetValues(typeof(ProductCategory)))
        {
            InvalidateCategoryCache(category);
        }

        // Invalidate all products cache
        _cache.Remove(GetAllProductsKey());
        _logger.LogInformation("Invalidated all product caches");
    }

    /// <summary>
    /// Gets statistics about the current cache state.
    /// </summary>
    /// <returns>Dictionary containing cache statistics.</returns>
    public Dictionary<string, object> GetCacheStats()
    {
        var stats = new Dictionary<string, object>();
        
        foreach (ProductCategory category in Enum.GetValues(typeof(ProductCategory)))
        {
            var key = GetCategoryKey(category);
            var isCached = _cache.TryGetValue(key, out List<Product>? products);
            stats[category.ToString()] = new
            {
                Cached = isCached,
                Count = products?.Count ?? 0
            };
        }

        return stats;
    }
}

