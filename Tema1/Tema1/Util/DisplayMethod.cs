namespace Tema1.Util;

public static class DisplayMethod
{
    public static void DisplayObject(object obj)
    {
        switch (obj)
        {
            case Book book:
                Console.WriteLine($"Book: '{book.Title}' published in {book.YearPublished} by {book.Author}");
                break;
            case Borrower borrower:
                Console.WriteLine($"Borrower: {borrower.Name} has {borrower.BorrowedBooks.Count} borrowed book(s)");
                break;
            default:
                Console.WriteLine("Unknown object type");
                break;
        }
    }
}