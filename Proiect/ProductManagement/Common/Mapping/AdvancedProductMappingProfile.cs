using AutoMapper;
using ProductManagement.Common.Resolvers;
using ProductManagement.Features.Products;
using ProductManagement.Features.Products.CreateProduct;

namespace ProductManagement.Common.Mapping;

/// <summary>
/// AutoMapper profile for mapping between Product entities and DTOs with custom resolvers.
/// </summary>
public class AdvancedProductMappingProfile : Profile
{
    /// <summary>
    /// Configures all product-related mappings and resolvers.
    /// </summary>
    public AdvancedProductMappingProfile()
    {
        // Map CreateProductProfileRequest to Product
        CreateMap<CreateProductProfileRequest, Product>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.StockQuantity > 0))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom<ConditionalImageUrlResolver>())
            .ForMember(dest => dest.Price, opt => opt.MapFrom<ConditionalPriceResolver>())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        
        // Map Product to ProductProfileDto
        CreateMap<Product, ProductProfileDto>()
            .ForMember(dest => dest.CategoryDisplayName, opt => opt.MapFrom<CategoryDisplayResolver>())
            .ForMember(dest => dest.FormattedPrice, opt => opt.MapFrom<PriceFormatterResolver>())
            .ForMember(dest => dest.ProductAge, opt => opt.MapFrom<ProductAgeResolver>())
            .ForMember(dest => dest.BrandInitials, opt => opt.MapFrom<BrandInitialsResolver>())
            .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom<AvailabilityStatusResolver>());
    }
}