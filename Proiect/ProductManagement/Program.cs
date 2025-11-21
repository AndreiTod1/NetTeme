using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductManagement.Common.Mapping;
using ProductManagement.Features.Products;
using ProductManagement.Features.Products.CreateProduct;
using ProductManagement.Features.Products.BatchCreateProduct;
using ProductManagement.Features.Products.ProductMetrics;
using ProductManagement.Middleware;
using ProductManagement.Persistence;
using ProductManagement.Services;
using ProductManagement.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Product Management API",
            Version = "v1",
            Description = "API for managing products with advanced validation and logging.",
            Contact = new OpenApiContact
            {
                Name = "API Support",
                Email = "support@example.com"
            }
        });
});

builder.Services.AddEndpointsApiExplorer();

// Database
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlite("Data Source=productmanagement.db"));

// AutoMapper
builder.Services.AddAutoMapper(cfg => {}, typeof(AdvancedProductMappingProfile).Assembly);

// Memory Cache
builder.Services.AddMemoryCache();

// Services
builder.Services.AddScoped<ProductCacheService>();
builder.Services.AddScoped<ProductLocalizationService>();

// Handlers
builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<ProductMetricsHandler>();
builder.Services.AddScoped<BatchCreateProductHandler>();

// Validators
builder.Services.AddScoped<IValidator<CreateProductProfileRequest>, CreateProductProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductProfileValidator>();

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Management API V1");
        c.RoutePrefix = string.Empty;
        c.DisplayRequestDuration();
    });
    
    app.MapOpenApi();
}

// Middleware pipeline - Global exception handler must be first
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<CorrelationMiddleware>();
app.UseHttpsRedirection();

// Endpoints
app.MapPost("/products", async (
    CreateProductProfileRequest request,
    CreateProductHandler handler,
    ProductCacheService cacheService,
    CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(request, cancellationToken);
    
    // Invalidate category-specific cache
    cacheService.InvalidateCategoryCache(request.Category);
    
    return Results.Created($"/products/{result.Id}", result);
})
.WithName("CreateProduct")
.WithDescription("Create a new product with advanced validation and mapping")
.WithOpenApi();

app.MapPost("/products/batch", async (
    BatchCreateProductRequest request,
    BatchCreateProductHandler handler,
    ProductCacheService cacheService,
    CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(request, cancellationToken);
    
    // Invalidate all caches after batch operation
    cacheService.InvalidateAllCaches();
    
    return Results.Ok(result);
})
.WithName("BatchCreateProducts")
.WithDescription("Create multiple products in a single batch operation")
.WithOpenApi();

app.MapGet("/products/metrics", async (
    ProductMetricsHandler handler,
    CancellationToken cancellationToken) =>
{
    var metrics = await handler.GetMetrics(cancellationToken);
    return Results.Ok(metrics);
})
.WithName("GetProductMetrics")
.WithDescription("Get comprehensive product metrics and statistics")
.WithOpenApi();

app.MapGet("/products/cache/stats", (ProductCacheService cacheService) =>
{
    var stats = cacheService.GetCacheStats();
    return Results.Ok(stats);
})
.WithName("GetCacheStats")
.WithDescription("Get cache statistics for all product categories")
.WithOpenApi();

app.MapGet("/products/category/{category}", async (
    ProductCategory category,
    ApplicationContext context,
    ProductCacheService cacheService,
    CancellationToken cancellationToken) =>
{
    // Try to get from cache first
    var cachedProducts = cacheService.GetProductsByCategory(category);
    if (cachedProducts != null)
    {
        return Results.Ok(cachedProducts);
    }

    // Load from database
    var products = await context.Products
        .Where(p => p.Category == category)
        .ToListAsync(cancellationToken);

    // Cache the results
    cacheService.CacheProductsByCategory(category, products);

    return Results.Ok(products);
})
.WithName("GetProductsByCategory")
.WithDescription("Get products by category with caching support")
.WithOpenApi();

app.MapGet("/products/localized/{culture}", async (
    string culture,
    ApplicationContext context,
    ProductLocalizationService localizationService,
    CancellationToken cancellationToken) =>
{
    var products = await context.Products.ToListAsync(cancellationToken);
    
    var localizedProducts = products.Select(p => new
    {
        p.Id,
        p.Name,
        p.Brand,
        p.Sku,
        Category = localizationService.GetCategoryName(p.Category, culture),
        p.Price,
        p.ReleaseDate,
        p.ImageUrl,
        StockStatus = localizationService.GetStockStatus(p.StockQuantity, culture),
        p.StockQuantity
    });

    return Results.Ok(localizedProducts);
})
.WithName("GetLocalizedProducts")
.WithDescription("Get products with localized category and stock status (en, ro, fr)")
.WithOpenApi();

app.Run();
