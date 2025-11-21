using AutoMapper;
using ProductManagement.Features.Products;
using ProductManagement.Features.Products.CreateProduct;

namespace ProductManagement.Common.Resolvers;

public class PriceFormatterResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        return source.Price.ToString("C2");
    }
}