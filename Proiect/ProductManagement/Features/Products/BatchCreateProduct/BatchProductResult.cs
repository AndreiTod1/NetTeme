namespace ProductManagement.Features.Products.BatchCreateProduct;

public class BatchProductResult
{
    public int Index { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? ProductId { get; set; }
    public string? Sku { get; set; }
}

