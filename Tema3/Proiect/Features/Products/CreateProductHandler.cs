using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Tema3.Common.Logging;
using Tema3.Features.Products.DTO;

namespace Tema3.Features.Products;

public class CreateProductHandler
{
    private readonly ILogger<CreateProductHandler> _logger;
    private readonly IValidator<CreateProductProfileRequest> _validator;

    public CreateProductHandler(
        ILogger<CreateProductHandler> logger,
        IValidator<CreateProductProfileRequest> validator)
    {
        _logger = logger;
        _validator = validator;
    }

    public async Task Handle(CreateProductProfileRequest request, CancellationToken cancellationToken)
    {
        var operationId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;
        var validationStart = DateTime.UtcNow;

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        var validationDuration = DateTime.UtcNow - validationStart;

        if (!validationResult.IsValid)
        {
            var totalDuration = DateTime.UtcNow - startTime;
            var metrics = new ProductCreationMetrics(
                operationId,
                request.Name,
                request.SKU,
                request.Category,
                validationDuration,
                TimeSpan.Zero,
                totalDuration,
                false,
                "Validation Failed"
            );

            _logger.LogProductCreationMetrics(metrics);
            return;
        }

        var dbSaveStart = DateTime.UtcNow;
        // Simulate database save with delay
        await Task.Delay(100, cancellationToken);
        var dbSaveDuration = DateTime.UtcNow - dbSaveStart;

        var totalDurationFinal = DateTime.UtcNow - startTime;
        var successMetrics = new ProductCreationMetrics(
            operationId,
            request.Name,
            request.SKU,
            request.Category,
            validationDuration,
            dbSaveDuration,
            totalDurationFinal,
            true
        );

        _logger.LogProductCreationMetrics(successMetrics);
    }
}