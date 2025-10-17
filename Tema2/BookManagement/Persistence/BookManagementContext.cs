using Microsoft.EntityFrameworkCore;
namespace BookManagement.Persistence;

public class BookManagementContext(DbContextOptions<BookManagementContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Book>()
            .Property(b => b.Id)
            .ValueGeneratedOnAdd();
    }

}