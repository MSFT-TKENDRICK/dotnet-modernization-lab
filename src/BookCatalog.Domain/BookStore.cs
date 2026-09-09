namespace BookCatalog.Domain;

public sealed class BookStore
{
    private readonly object _gate = new();
    private readonly List<Book> _books =
    [
        new(1, "The Lantern Atlas", "Mira Vale"),
        new(2, "Gardens of Glass", "Rowan Ash"),
        new(3, "The Clockwork Orchard", "Ellis North")
    ];
    private int _nextId = 4;

    public Book[] GetAll()
    {
        lock (_gate)
        {
            return _books.OrderBy(book => book.Id).ToArray();
        }
    }

    public Book? GetById(int id)
    {
        lock (_gate)
        {
            return _books.Find(book => book.Id == id);
        }
    }

    public Book Add(string title, string author)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(author);

        lock (_gate)
        {
            var book = new Book(_nextId++, title, author);
            _books.Add(book);
            return book;
        }
    }
}
