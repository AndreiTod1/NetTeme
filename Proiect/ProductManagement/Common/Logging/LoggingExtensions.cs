namespace ProductManagement.Common.Logging;
using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// Extension methods for logging product-related metrics and events.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Logs detailed metrics for product creation operations.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="metrics">The product creation metrics to log.</param>
    public static void LogProductCreationMetrics(
        this ILogger logger,
        ProductCreationMetrics metrics)
    {
        var message =
            $"[ProductCreationMetrics] OperationId={metrics.OperationId}, " +
            $"Name={metrics.ProductName}, SKU={metrics.Sku}, Category={metrics.Category}, " +
            $"Validation={metrics.ValidationDuration.TotalMilliseconds}ms, " +
            $"DBSave={metrics.DatabaseSaveDuration.TotalMilliseconds}ms, " +
            $"Total={metrics.TotalDuration.TotalMilliseconds}ms, " +
            $"Success={metrics.Success}, " +
            $"ErrorReason={(metrics.ErrorReason ?? "None")}";

        logger.LogInformation(ProductLogEvents.ProductCreationCompleted, message);
    }
}