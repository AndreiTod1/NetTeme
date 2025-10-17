using BookManagement.Exceptions;
using BookManagement.Features.Books;
using BookManagement.Persistence;

public class GetBookByIdHandler
{
    private readonly BookManagementContext _context;

    public GetBookByIdHandler(BookManagementContext context) => _context = context;

    public async Task<IResult> Handle(GetBookByIdRequest request)
    {
        var book = await _context.Books.FindAsync(request.Id);

        if (book is null)
            throw new BookNotFoundException(request.Id); 

        return Results.Ok(book);
    }
}