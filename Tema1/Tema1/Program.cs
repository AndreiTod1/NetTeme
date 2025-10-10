using Tema1;
using Tema1.Util;
using Tema1.Services;

var books = new List<Book>
{
    new Book( "Book1", "Author1", 2005),
    new Book( "Book2", "Author2", 2015),
    new Book( "Book3", "Author3", 2020),
    new Book( "Book4", "Author4", 1999),
    new Book( "Book5", "Author5", 2011)
};

var librarian = new Librarian("Librarian 1", "lib1@gmail.com", "Science Fiction");

Console.WriteLine(librarian);
Console.WriteLine();



while (true)
{
    Console.WriteLine("\n=== MENU ===");
    Console.WriteLine("1. Add book");
    Console.WriteLine("2. Display books");
    Console.WriteLine("3. Display new Books");
    Console.WriteLine("4. Demo with expression");
    Console.WriteLine("5. Exit");
    Console.Write("Choose an option: ");
    var choice = Console.ReadLine();
    Console.WriteLine();

    switch (choice)
    {
        case "1": 
            Console.Write("Title: ");
            var title = Console.ReadLine();
            
            Console.Write("Author: ");
            var author = Console.ReadLine();
            
            Console.Write("Year: ");
            if (int.TryParse(Console.ReadLine(), out int year))
            {
                var book = new Book(title, author, year);
                books.Add(book);
                Console.WriteLine("Book added!\n");
                DisplayMethod.DisplayObject(book);
            }
            else
            {
                Console.WriteLine("Invalid year!");
            }
            break;
        
        case "2":
            Console.WriteLine("Books:");
            foreach (var book in books)
            {
                DisplayMethod.DisplayObject(book);
            }
            break;
        case "3":
            var newBooks = BookService.GetNewBooks(books);
            Console.WriteLine("New Books (after 2010):");
            foreach (var book in newBooks)
            {
                DisplayMethod.DisplayObject(book);
            }
            break;
        
        case "4":
            var borrower = new Borrower(1, "John Doe", new List<Book> {books[0]});
            DisplayMethod.DisplayObject(borrower);
            
            
            var updatedBorrower = BorrowerService.AddBook(borrower, books[1]);
            Console.WriteLine("Has borrowed a new book: " + books[1].Title);
            DisplayMethod.DisplayObject(updatedBorrower);
            break;
        case "5":
            Console.WriteLine("Exiting...");
            return;
        default:
            Console.WriteLine("Invalid option");
            break;  
    }
    
    
}

