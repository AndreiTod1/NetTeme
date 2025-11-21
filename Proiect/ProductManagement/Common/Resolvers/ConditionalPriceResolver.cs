using AutoMapper;
using ProductManagement.Features.Products;
using ProductManagement.Features.Products.CreateProduct;

namespace ProductManagement.Common.Resolvers;

public class ConditionalPriceResolver : IValueResolver<CreateProductProfileRequest, Product, decimal>
{
    public decimal Resolve(CreateProductProfileRequest source, Product destination, decimal destMember, ResolutionContext context)
    {
        // Apply 10% discount for Home category
        if (source.Category == ProductCategory.Home)
        {
            return source.Price * 0.9m;
        }
        
        // Return actual price for all other categories
        return source.Price;
    }
}
