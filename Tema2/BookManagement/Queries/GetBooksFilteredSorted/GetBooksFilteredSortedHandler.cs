using BookManagement.Persistence;
using Microsoft.EntityFrameworkCore;


public class GetBooksFilteredSortedHandler
{
    private readonly BookManagementContext _context;

    public GetBooksFilteredSortedHandler(BookManagementContext context)
        => _context = context;

    public async Task<IResult> Handle(
        string? author,
        string? sortBy,
        bool descending,
        int page,
        int pageSize)
    {
       
        if (page <= 0)
            return Results.BadRequest(new { Message = "Page number must be greater than 0." });

        if (pageSize <= 0)
            return Results.BadRequest(new { Message = "PageSize must be greater than 0." });

        if (pageSize > 100)
            return Results.BadRequest(new { Message = "PageSize cannot exceed 100." });

        string? sortProperty = sortBy?.ToLower();
        var allowedSorts = new[] { "title", "year" };

        if (!string.IsNullOrWhiteSpace(sortProperty) && !allowedSorts.Contains(sortProperty))
            return Results.BadRequest(new { Message = $"Invalid sort property. Allowed values: {string.Join(", ", allowedSorts)}" });

       
        var booksQuery = _context.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(author))
            booksQuery = booksQuery.Where(b => b.Author.Contains(author));

  
        booksQuery = sortProperty switch
        {
            "title" => descending ? booksQuery.OrderByDescending(b => b.Title)
                                  : booksQuery.OrderBy(b => b.Title),
            "year"  => descending ? booksQuery.OrderByDescending(b => b.YearPublished)
                                  : booksQuery.OrderBy(b => b.YearPublished),
            _ => booksQuery.OrderBy(b => b.Id) // default sort
        };

        var totalCount = await booksQuery.CountAsync();
        var skip = (page - 1) * pageSize;

        if (totalCount > 0 && skip >= totalCount)
            return Results.BadRequest(new { Message = "Page exceeds total number of records." });

        var items = await booksQuery.Skip(skip).Take(pageSize).ToListAsync();

        
        return Results.Ok(new
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        });
    }
}
