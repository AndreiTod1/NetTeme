using BookManagement.Persistence;
using Microsoft.EntityFrameworkCore;


public class GetPaginatedBooksHandler
{
    private readonly BookManagementContext _context;

    public GetPaginatedBooksHandler(BookManagementContext context)
        => _context = context;

    public async Task<IResult> Handle(GetPaginatedBooksRequest request)
    {
        if (request.Page <= 0)
            return Results.BadRequest(new { Message = "Page must be greater than zero." });

        if (request.PageSize <= 0)
            return Results.BadRequest(new { Message = "PageSize must be greater than zero." });

        if (request.PageSize > 100)
            return Results.BadRequest(new { Message = "PageSize cannot exceed 100." });

        var totalCount = await _context.Books.CountAsync();
        var skip = (request.Page - 1) * request.PageSize;
        if (totalCount > 0 && skip >= totalCount)
            return Results.BadRequest(new { Message = "Page exceeds total number of records." });

        var books = await _context.Books
            .OrderBy(b => b.Id)
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync();

        var response = new
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Items = books
        };

        return Results.Ok(response);
    }
}
