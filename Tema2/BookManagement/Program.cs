using BookManagement.Features.Books;
using BookManagement.Middlewares;
using BookManagement.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();


builder.Services.AddDbContext<BookManagementContext>(options => 
    options.UseSqlite("Data Source=bookmanagement.db"));


builder.Services.AddScoped<CreateBookHandler>();
builder.Services.AddScoped<GetAllBooksHandler>();
builder.Services.AddScoped<GetBookByIdHandler>();
builder.Services.AddScoped<UpdateBookHandler>();
builder.Services.AddScoped<DeleteBookByIdHandler>();
builder.Services.AddScoped<GetPaginatedBooksHandler>();
builder.Services.AddScoped<GetBooksFilteredSortedHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookManagementContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();


// Test
app.MapGet("/test", () => "API is alive!");

// Create
app.MapPost("/books", async (CreateBookRequest request, CreateBookHandler handler) =>
    await handler.Handle(request));

// Get All
app.MapGet("/books", async (GetAllBooksHandler handler) =>
    await handler.Handle(new GetAllBooksRequest()));

// Get By Id
app.MapGet("/books/{id}", async (int id, GetBookByIdHandler handler) =>
    await handler.Handle(new GetBookByIdRequest(id)));

// Update
app.MapPut("/books/{id}", async (int id, UpdateBookRequest request, UpdateBookHandler handler) =>
{
    return await handler.Handle(id, request);
});

// Delete
app.MapDelete("/books/{id}", async (int id, DeleteBookByIdHandler handler) =>
    await handler.Handle(new DeleteBookByIdRequest(id)));

// Pagination
app.MapGet("/books/paged", async (int page, int pageSize, GetPaginatedBooksHandler handler) =>
    await handler.Handle(new GetPaginatedBooksRequest(page, pageSize)));


// Filtering, Sorting, and Pagination
// Example: /books/search?author=John&sortBy=Title&descending=true&page=1&pageSize=5
// This endpoint allows filtering by author, sorting by title or year, and pagination.
app.MapGet("/books/search", async (
    GetBooksFilteredSortedHandler handler,
    string? author = null,
    string? sortBy = null,
    bool descending = false,
    int page = 1,
    int pageSize = 10) =>
{
    return await handler.Handle(author, sortBy, descending, page, pageSize);
});



app.Run();
