    using BookManagement.Persistence;

    namespace BookManagement.Features.Books;

    public class CreateBookHandler
    {
        private readonly BookManagementContext _context;

        public CreateBookHandler(BookManagementContext context) => _context = context;

        public async Task<IResult> Handle(CreateBookRequest request)
        {
            var book = new Book(request.Title, request.Author, request.YearPublished);

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return Results.Created($"/books/{book.Id}", book);
        }
    }