namespace Tema3.Common.Logging;
using System;
using Microsoft.Extensions.Logging;

public static class LoggingExtensions
{
    public static void LogProductCreationMetrics(
        this ILogger logger,
        ProductCreationMetrics metrics)
    {
        var message =
            $"[ProductCreationMetrics] OperationId={metrics.OperationId}, " +
            $"Name={metrics.ProductName}, SKU={metrics.SKU}, Category={metrics.Category}, " +
            $"Validation={metrics.ValidationDuration.TotalMilliseconds}ms, " +
            $"DBSave={metrics.DatabaseSaveDuration.TotalMilliseconds}ms, " +
            $"Total={metrics.TotalDuration.TotalMilliseconds}ms, " +
            $"Success={metrics.Success}, " +
            $"ErrorReason={(metrics.ErrorReason ?? "None")}";

        logger.LogInformation(ProductLogEvents.ProductCreationCompleted, message);
    }
}