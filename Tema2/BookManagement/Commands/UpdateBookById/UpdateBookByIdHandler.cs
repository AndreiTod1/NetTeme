using BookManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Features.Books;

public class UpdateBookHandler
{
    private readonly BookManagementContext _context;

    public UpdateBookHandler(BookManagementContext context) => _context = context;

    public async Task<IResult> Handle(int id, UpdateBookRequest request)
    {
        var book = await _context.Books.FindAsync(id);
        if (book is null)
            return Results.NotFound(new { Message = $"Book with Id {id} not found." });

        book.Title = request.Title;
        book.Author = request.Author;
        book.YearPublished = request.YearPublished;

        await _context.SaveChangesAsync();
        return Results.Ok(book);
    }
}
