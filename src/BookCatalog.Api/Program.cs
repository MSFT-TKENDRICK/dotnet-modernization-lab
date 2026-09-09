using BookCatalog.Api;
using BookCatalog.Domain;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<BookStore>();

var app = builder.Build();

app.MapGet("/books", (BookStore books) => Results.Ok(books.GetAll()));

app.MapGet("/books/{id:int}", (int id, BookStore books) =>
    books.GetById(id) is { } book ? Results.Ok(book) : Results.NotFound());

app.MapPost("/books", (CreateBookRequest request, BookStore books) =>
{
    var errors = new Dictionary<string, string[]>();
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        errors["title"] = ["Title is required."];
    }

    if (string.IsNullOrWhiteSpace(request.Author))
    {
        errors["author"] = ["Author is required."];
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var book = books.Add(request.Title!, request.Author!);
    return Results.Created($"/books/{book.Id}", book);
});

app.Run();

public partial class Program;
