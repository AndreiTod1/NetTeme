using Microsoft.EntityFrameworkCore;
using Tema3.Features.Products;

namespace Tema3.Validators;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        
    }

    public DbSet<Product> Products { get; set; }
}