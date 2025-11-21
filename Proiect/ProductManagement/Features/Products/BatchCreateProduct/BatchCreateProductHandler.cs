using AutoMapper;
using FluentValidation;
using ProductManagement.Features.Products.CreateProduct;
using ProductManagement.Persistence;
using System.Diagnostics;

namespace ProductManagement.Features.Products.BatchCreateProduct;

public class BatchCreateProductHandler
{
    private readonly ApplicationContext _context;
    private readonly IValidator<CreateProductProfileRequest> _validator;
    private readonly IMapper _mapper;
    private readonly ILogger<BatchCreateProductHandler> _logger;

    public BatchCreateProductHandler(
        ApplicationContext context,
        IValidator<CreateProductProfileRequest> validator,
        IMapper mapper,
        ILogger<BatchCreateProductHandler> logger)
    {
        _context = context;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BatchCreateProductResponse> Handle(
        BatchCreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var operationId = Guid.NewGuid().ToString()[..8];

        _logger.LogInformation(
            "[{OperationId}] Starting batch product creation: {Count} products requested",
            operationId, request.Products.Count);

        var response = new BatchCreateProductResponse
        {
            TotalRequested = request.Products.Count
        };

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var semaphore = new SemaphoreSlim(5);
            var tasks = request.Products.Select(async (productRequest, index) =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await ProcessSingleProduct(productRequest, index, operationId, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToArray();

            var results = await Task.WhenAll(tasks);
            response.Results = results.ToList();

            response.SuccessCount = results.Count(r => r.Success);
            response.FailureCount = results.Count(r => !r.Success);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "[{OperationId}] Batch operation completed: {Success} successful, {Failed} failed",
                operationId, response.SuccessCount, response.FailureCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex,
                "[{OperationId}] Batch operation failed, transaction rolled back",
                operationId);
            throw;
        }

        stopwatch.Stop();
        response.TotalDuration = stopwatch.Elapsed;

        _logger.LogInformation(
            "[{OperationId}] Batch operation duration: {Duration}ms",
            operationId, response.TotalDuration.TotalMilliseconds);

        return response;
    }

    private async Task<BatchProductResult> ProcessSingleProduct(
        CreateProductProfileRequest productRequest,
        int index,
        string operationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(productRequest, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning(
                    "[{OperationId}] Product at index {Index} validation failed: {Errors}",
                    operationId, index, errors);

                return new BatchProductResult
                {
                    Index = index,
                    Success = false,
                    ErrorMessage = errors,
                    Sku = productRequest.Sku
                };
            }

            var product = _mapper.Map<Product>(productRequest);
            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[{OperationId}] Product at index {Index} created: {ProductId}",
                operationId, index, product.Id);

            return new BatchProductResult
            {
                Index = index,
                Success = true,
                ProductId = product.Id,
                Sku = product.Sku
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[{OperationId}] Error processing product at index {Index}",
                operationId, index);

            return new BatchProductResult
            {
                Index = index,
                Success = false,
                ErrorMessage = ex.Message,
                Sku = productRequest.Sku
            };
        }
    }
}
