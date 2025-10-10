namespace Tema1.Services;

public static class BookService
{
    public static Func<Book, bool> isNewBook = static book => book.YearPublished > 2010;

    public static List<Book> GetNewBooks(List<Book> books)
    {
        return books.Where(isNewBook).ToList();
    }
}