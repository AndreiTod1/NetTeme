namespace BookManagement.Exceptions;

public class InvalidSortException : Exception
{
    public InvalidSortException(string param) : base($"Invalid sort: {param}. Use: title, year, author") { }
}