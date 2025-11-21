using ProductManagement.Features.Products.CreateProduct;

namespace ProductManagement.Features.Products.BatchCreateProduct;

public class BatchCreateProductRequest
{
    public List<CreateProductProfileRequest> Products { get; set; } = new();
}

