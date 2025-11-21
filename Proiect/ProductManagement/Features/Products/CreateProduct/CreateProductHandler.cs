using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using ProductManagement.Common.Logging;
using ProductManagement.Persistence;
using ValidationException = ProductManagement.Exceptions.ValidationException;

namespace ProductManagement.Features.Products.CreateProduct;

/// <summary>
/// Handles the creation of new products with validation, mapping, and caching.
/// </summary>
public class CreateProductHandler
{
    private readonly ILogger<CreateProductHandler> _logger;
    private readonly IValidator<CreateProductProfileRequest> _validator;
    private readonly ApplicationContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    public CreateProductHandler(
        ILogger<CreateProductHandler> logger,
        IValidator<CreateProductProfileRequest> validator,
        ApplicationContext context,
        IMapper mapper,
        IMemoryCache cache)
    {
        _logger = logger;
        _validator = validator;
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }

    /// <summary>
    /// Creates a new product in the system.
    /// </summary>
    /// <param name="request">The product creation request containing product details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created product profile with all computed fields.</returns>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public async Task<ProductProfileDto> Handle(CreateProductProfileRequest request, CancellationToken cancellationToken)
    {
        var operationId = Guid.NewGuid().ToString()[..8];
        var startTime = DateTime.UtcNow;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["OperationId"] = operationId,
            ["ProductName"] = request.Name,
            ["Brand"] = request.Brand,
            ["SKU"] = request.Sku,
            ["Category"] = request.Category
        }))
        {
            _logger.LogInformation(
                ProductLogEvents.ProductCreationStarted,
                "Starting product creation: Name={Name}, Brand={Brand}, SKU={SKU}, Category={Category}",
                request.Name, request.Brand, request.Sku, request.Category);

            try
            {
                var validationStart = DateTime.UtcNow;
                
                _logger.LogInformation(ProductLogEvents.SKUValidationPerformed, "Performing SKU validation for SKU={SKU}", request.Sku);
                _logger.LogInformation(ProductLogEvents.StockValidationPerformed, "Performing stock validation: StockQuantity={StockQuantity}", request.StockQuantity);

                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                var validationDuration = DateTime.UtcNow - validationStart;

                if (!validationResult.IsValid)
                {
                    var totalDuration = DateTime.UtcNow - startTime;
                    var errorReason = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                    
                    var failMetrics = new ProductCreationMetrics(operationId, request.Name, request.Sku, request.Category, validationDuration, TimeSpan.Zero, totalDuration, false, errorReason);
                    _logger.LogProductCreationMetrics(failMetrics);
                    _logger.LogWarning(ProductLogEvents.ProductValidationFailed, "Product validation failed: {Errors}", errorReason);
                    
                    throw new ValidationException(errorReason);
                }

                var dbSaveStart = DateTime.UtcNow;
                _logger.LogInformation(ProductLogEvents.DatabaseOperationStarted, "Starting database operation for product creation");

                var product = _mapper.Map<Product>(request);
                await _context.Products.AddAsync(product, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                
                var dbSaveDuration = DateTime.UtcNow - dbSaveStart;
                _logger.LogInformation(ProductLogEvents.DatabaseOperationCompleted, "Database operation completed: ProductId={ProductId}", product.Id);

                _cache.Remove("all_products");
                _logger.LogInformation(ProductLogEvents.CacheOperationPerformed, "Cache invalidated: CacheKey={CacheKey}", "all_products");

                var productDto = _mapper.Map<ProductProfileDto>(product);

                var totalDurationFinal = DateTime.UtcNow - startTime;
                var successMetrics = new ProductCreationMetrics(operationId, request.Name, request.Sku, request.Category, validationDuration, dbSaveDuration, totalDurationFinal, true);
                _logger.LogProductCreationMetrics(successMetrics);

                return productDto;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var totalDuration = DateTime.UtcNow - startTime;
                var errorMetrics = new ProductCreationMetrics(operationId, request.Name, request.Sku, request.Category, TimeSpan.Zero, TimeSpan.Zero, totalDuration, false, ex.Message);
                _logger.LogProductCreationMetrics(errorMetrics);
                throw;
            }
        }
    }
}
