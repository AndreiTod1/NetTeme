namespace Tema1.Services;

public static class BorrowerService
{
    public static Borrower AddBook(Borrower borrower, Book book)
    {
        var newBooks = new List<Book>(borrower.BorrowedBooks) { book };
        return borrower with { BorrowedBooks = newBooks };
    }
}