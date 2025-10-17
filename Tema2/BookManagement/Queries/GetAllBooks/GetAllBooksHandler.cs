using BookManagement.Persistence;
using Microsoft.EntityFrameworkCore;


public class GetAllBooksHandler
{
    private readonly BookManagementContext _context;

    public GetAllBooksHandler(BookManagementContext context) => _context = context;

    public async Task<IResult> Handle(GetAllBooksRequest request)
    {
        var books = await _context.Books.ToListAsync();
        return Results.Ok(books);
    }
}