using Microsoft.EntityFrameworkCore;
using ProductManagement.Features.Products;

namespace ProductManagement.Persistence;

/// <summary>
/// Database context for the Product Management application.
/// </summary>
public class ApplicationContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the ApplicationContext.
    /// </summary>
    /// <param name="options">The options for configuring the context.</param>
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Products DbSet.
    /// </summary>
    public DbSet<Product> Products { get; set; }
}