using BookManagement.Exceptions;
using BookManagement.Persistence;

namespace BookManagement.Features.Books;

public class DeleteBookByIdHandler
{
    private readonly BookManagementContext _context;

    public DeleteBookByIdHandler(BookManagementContext context) => _context = context;

    public async Task<IResult> Handle(DeleteBookByIdRequest request)
    {
        var book = await _context.Books.FindAsync(request.Id);
        if (book is null)
            throw new BookNotFoundException(request.Id);

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return Results.NoContent();
    }
}