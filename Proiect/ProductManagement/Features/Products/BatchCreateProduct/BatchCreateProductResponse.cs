namespace ProductManagement.Features.Products.BatchCreateProduct;

public class BatchCreateProductResponse
{
    public int TotalRequested { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BatchProductResult> Results { get; set; } = new();
    public TimeSpan TotalDuration { get; set; }
}

