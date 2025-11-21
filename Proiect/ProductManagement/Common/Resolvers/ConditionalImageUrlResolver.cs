using AutoMapper;
using ProductManagement.Features.Products;
using ProductManagement.Features.Products.CreateProduct;

namespace ProductManagement.Common.Resolvers;

public class ConditionalImageUrlResolver : IValueResolver<CreateProductProfileRequest, Product, string?>
{
    public string? Resolve(CreateProductProfileRequest source, Product destination, string? destMember, ResolutionContext context)
    {
        // Return null for Home category (content filtering)
        if (source.Category == ProductCategory.Home)
        {
            return null;
        }
        
        // Return actual URL for Electronics, Clothing, Books categories
        return source.ImageUrl;
    }
}