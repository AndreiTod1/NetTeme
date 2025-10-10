namespace Tema1;

public class Librarian
{
    public string name { get; init; }
    public string email { get; init; }
    public string librarySection { get; init; }
    
    public Librarian (string name, string email, string librarySection)
    {
        this.name = name;
        this.email = email;
        this.librarySection = librarySection;
    }
    
    public override string ToString()
    {
        return $"Librarian: {name}, Email: {email}, Section: {librarySection}";
    }
}