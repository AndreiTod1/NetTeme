using ProductManagement.Features.Products;

namespace ProductManagement.Common.Logging;

/// <summary>
/// Records metrics and performance data for product creation operations.
/// </summary>
/// <param name="OperationId">Unique identifier for this operation.</param>
/// <param name="ProductName">Name of the product being created.</param>
/// <param name="Sku">SKU of the product being created.</param>
/// <param name="Category">Category of the product being created.</param>
/// <param name="ValidationDuration">Time spent on validation.</param>
/// <param name="DatabaseSaveDuration">Time spent saving to database.</param>
/// <param name="TotalDuration">Total time for the operation.</param>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="ErrorReason">Error message if the operation failed.</param>
public record ProductCreationMetrics(
    string OperationId,
    string ProductName,
    string Sku,
    ProductCategory Category,
    TimeSpan ValidationDuration,
    TimeSpan DatabaseSaveDuration,
    TimeSpan TotalDuration,
    bool Success,
    string? ErrorReason = null
);