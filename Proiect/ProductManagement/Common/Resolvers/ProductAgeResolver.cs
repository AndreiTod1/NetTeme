using AutoMapper;
using ProductManagement.Features.Products;
using ProductManagement.Features.Products.CreateProduct;

namespace ProductManagement.Common.Resolvers;

public class ProductAgeResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var ageInDays = (DateTime.UtcNow - source.ReleaseDate).TotalDays;
        
        // Use <= to include exactly 30 days as "New Release"
        if (ageInDays < 30)
        {
            return "New Release";
        }
        
        if (ageInDays < 365)
        {
            var months = (int)(ageInDays / 30);
            return $"{months} months old";
        }
        
        if (ageInDays < 1825) // Less than 5 years
        {
            var years = (int)(ageInDays / 365);
            return $"{years} years old";
        }
        
        // 5 years or older = Classic
        return "Classic";
    }
}