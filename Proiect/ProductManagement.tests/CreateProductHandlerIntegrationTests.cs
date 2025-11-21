using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProductManagement.Common.Mapping;
using ProductManagement.Features.Products;
using ProductManagement.Features.Products.CreateProduct;
using ProductManagement.Persistence;
using ProductManagement.Validators;
using Xunit;
using ValidationException = ProductManagement.Exceptions.ValidationException;

namespace ProductManagement.tests;

public class CreateProductHandlerIntegrationTests : IDisposable
{
    private readonly ApplicationContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<CreateProductHandler>> _handlerLoggerMock;
    private readonly Mock<ILogger<CreateProductProfileValidator>> _validatorLoggerMock;
    private readonly CreateProductHandler _handler;
    
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile(new AdvancedProductMappingProfile()), new NullLoggerFactory());
        return cfg.CreateMapper();
    }

    public CreateProductHandlerIntegrationTests()
    {
        // Set up in-memory database with unique name
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationContext(options);

        // Configure AutoMapper with both product profiles
        _mapper = CreateMapper();

        // Set up memory cache
        _cache = new MemoryCache(new MemoryCacheOptions());

        // Mock loggers
        _handlerLoggerMock = new Mock<ILogger<CreateProductHandler>>();
        _validatorLoggerMock = new Mock<ILogger<CreateProductProfileValidator>>();

        // Create validator
        var validator = new CreateProductProfileValidator(_context, _validatorLoggerMock.Object);

        // Create handler instance with all dependencies
        _handler = new CreateProductHandler(
            _handlerLoggerMock.Object,
            validator,
            _context,
            _mapper,
            _cache);
    }

    [Fact]
    public async Task Handle_ValidElectronicsProductRequest_CreatesProductWithCorrectMappings()
    {
        // Arrange: Create valid Electronics product request with all properties
        var request = new CreateProductProfileRequest
        {
            Name = "Smart Laptop Computer",
            Brand = "Tech Brand",
            Sku = "ELEC-12345",
            Category = ProductCategory.Electronics,
            Price = 999.99m,
            ReleaseDate = DateTime.UtcNow.AddDays(-29),
            ImageUrl = "https://example.com/laptop.jpg",
            StockQuantity = 10
        };

        // Act: Call handler
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert: Verify Created result type
        Assert.NotNull(result);
        Assert.IsType<ProductProfileDto>(result);

        // Assert: Check CategoryDisplayName = "Electronics & Technology"
        Assert.Equal("Electronics & Technology", result.CategoryDisplayName);

        // Assert: Check BrandInitials for two-word brand
        Assert.Equal("TB", result.BrandInitials);

        // Assert: Check ProductAge calculation
        Assert.Equal("New Release", result.ProductAge);

        // Assert: Check FormattedPrice starts with currency symbol
        Assert.StartsWith("$", result.FormattedPrice, StringComparison.Ordinal);
        Assert.Contains("999.99", result.FormattedPrice, StringComparison.Ordinal);

        // Assert: Check AvailabilityStatus based on stock
        Assert.Equal("In Stock", result.AvailabilityStatus);

        // Assert: Verify ProductCreationStarted log called once
        _handlerLoggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == 2001),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSKU_ThrowsValidationExceptionWithLogging()
    {
        // Arrange: Create existing product in database with specific SKU
        var existingProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Existing Product",
            Brand = "Some Brand",
            Sku = "DUP-SKU-123",
            Category = ProductCategory.Books,
            Price = 50m,
            ReleaseDate = DateTime.UtcNow.AddDays(-100),
            StockQuantity = 5,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Products.AddAsync(existingProduct);
        await _context.SaveChangesAsync();

        // Arrange: Create request with same SKU
        var request = new CreateProductProfileRequest
        {
            Name = "New Product",
            Brand = "Different Brand",
            Sku = "DUP-SKU-123",
            Category = ProductCategory.Electronics,
            Price = 100m,
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            ImageUrl = "https://example.com/product.jpg",
            StockQuantity = 3
        };

        // Act & Assert: Verify ValidationException thrown
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(request, CancellationToken.None));

        // Assert: Check exception message contains "already exists"
        Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);

        // Assert: Verify ProductValidationFailed log called once
        _handlerLoggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.Is<EventId>(e => e.Id == 2002),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_HomeProductRequest_AppliesDiscountAndConditionalMapping()
    {
        // Arrange: Create valid Home product request
        var request = new CreateProductProfileRequest
        {
            Name = "Cozy Sofa",
            Brand = "Home Comfort",
            Sku = "HOME-98765",
            Category = ProductCategory.Home,
            Price = 180m,
            ReleaseDate = DateTime.UtcNow.AddYears(-2),
            ImageUrl = "https://example.com/sofa.jpg",
            StockQuantity = 3
        };

        // Act: Call handler
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert: Check CategoryDisplayName = "Home & Garden"
        Assert.Equal("Home & Garden", result.CategoryDisplayName);

        // Assert: Check Price has 10% discount applied
        var expectedDiscountedPrice = 180m * 0.9m; // 162m
        Assert.Equal(expectedDiscountedPrice, result.Price);

        // Assert: Check ImageUrl is null (content filtering)
        Assert.Null(result.ImageUrl);
    }

    public void Dispose()
    {
        // Proper disposal of context and cache
        _context.Dispose();
        _cache.Dispose();
    }
}
