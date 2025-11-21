using AutoMapper;
using ProductManagement.Features.Products;
using ProductManagement.Features.Products.CreateProduct;

namespace ProductManagement.Common.Resolvers;

public class BrandInitialsResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var brand = source.Brand;
        if (string.IsNullOrWhiteSpace(brand))
        {
            return "?";
        }

        var words = brand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 1)
        {
            return words[0][0].ToString().ToUpper();
        }

        return (words[0][0].ToString() + words[^1][0].ToString()).ToUpper();
    }
}