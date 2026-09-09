using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BookCatalog.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BookCatalog.Api.Tests;

public sealed class BooksTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    private static Book[] ExpectedSeed =>
    [
        new(1, "The Lantern Atlas", "Mira Vale"),
        new(2, "Gardens of Glass", "Rowan Ash"),
        new(3, "The Clockwork Orchard", "Ellis North")
    ];

    public BooksTests()
    {
        // xUnit creates a test class instance per case, so no host state is shared.
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ListReturnsDeterministicSeedInIdOrder()
    {
        using var response = await _client.GetAsync("/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(ExpectedSeed, await response.Content.ReadFromJsonAsync<Book[]>());
    }

    [Fact]
    public async Task KnownIdReturnsMatchingBook()
    {
        using var response = await _client.GetAsync("/books/2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(ExpectedSeed[1], await response.Content.ReadFromJsonAsync<Book>());
    }

    [Theory]
    [InlineData(999)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UnknownIdReturnsNotFound(int id)
    {
        using var response = await _client.GetAsync($"/books/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("Maps of Tomorrow", "Tessa Reed")]
    [InlineData("  Maps of Tomorrow  ", "  Tessa Reed  ")]
    public async Task CreateReturnsLocationAndPersistsExactValues(string title, string author)
    {
        using var response = await _client.PostAsJsonAsync("/books", new { title, author });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Book>();
        Assert.Equal(new Book(4, title, author), created);
        Assert.Equal("/books/4", response.Headers.Location?.OriginalString);

        using var retrieved = await _client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, retrieved.StatusCode);
        Assert.Equal(created, await retrieved.Content.ReadFromJsonAsync<Book>());
        Assert.Equal(
            ExpectedSeed.Append(new Book(4, title, author)).ToArray(),
            await _client.GetFromJsonAsync<Book[]>("/books"));
    }

    [Theory]
    [InlineData("", "Tessa Reed", "title")]
    [InlineData(" \t\n", "Tessa Reed", "title")]
    [InlineData(null, "Tessa Reed", "title")]
    [InlineData("Maps of Tomorrow", "", "author")]
    [InlineData("Maps of Tomorrow", " \t\n", "author")]
    [InlineData("Maps of Tomorrow", null, "author")]
    [InlineData("", "", "title")]
    [InlineData(null, null, "author")]
    public async Task InvalidFieldsReturnValidationProblemWithoutMutation(
        string? title, string? author, string errorField)
    {
        using var response = await _client.PostAsJsonAsync("/books", new { title, author });

        await AssertValidationProblem(response, errorField);
        await AssertSeedUnchanged();
    }

    [Theory]
    [InlineData("{}", "title")]
    [InlineData("{\"author\":\"Tessa Reed\"}", "title")]
    [InlineData("{\"title\":\"Maps of Tomorrow\"}", "author")]
    public async Task MissingFieldsReturnValidationProblemWithoutMutation(
        string json, string errorField)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync("/books", content);

        await AssertValidationProblem(response, errorField);
        await AssertSeedUnchanged();
    }

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{\"title\":7,\"author\":\"Tessa Reed\"}")]
    public async Task InvalidJsonBodyReturnsBadRequestWithoutMutation(string json)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync("/books", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertSeedUnchanged();
    }

    [Fact]
    public async Task BothMissingFieldsHaveErrorsAndDoNotConsumeAnId()
    {
        using var rejected = await _client.PostAsJsonAsync("/books", new { });
        using var problem = await rejected.Content.ReadFromJsonAsync<JsonDocument>();

        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.NotNull(problem);
        var errors = problem.RootElement.GetProperty("errors");
        Assert.Equal(2, errors.EnumerateObject().Count());
        Assert.Equal("Title is required.", errors.GetProperty("title")[0].GetString());
        Assert.Equal("Author is required.", errors.GetProperty("author")[0].GetString());

        using var accepted = await _client.PostAsJsonAsync(
            "/books", new { title = "Maps of Tomorrow", author = "Tessa Reed" });
        Assert.Equal(HttpStatusCode.Created, accepted.StatusCode);
        Assert.Equal(4, (await accepted.Content.ReadFromJsonAsync<Book>())?.Id);
    }

    [Fact]
    public async Task SeparateHostsDoNotShareMutableState()
    {
        using var response = await _client.PostAsJsonAsync(
            "/books", new { title = "Maps of Tomorrow", author = "Tessa Reed" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var otherFactory = new WebApplicationFactory<Program>();
        using var otherClient = otherFactory.CreateClient();

        Assert.Equal(ExpectedSeed, await otherClient.GetFromJsonAsync<Book[]>("/books"));
        using var missing = await otherClient.GetAsync("/books/4");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(4, (await _client.GetFromJsonAsync<Book[]>("/books"))?.Length);
    }

    [Fact]
    public async Task ConcurrentCreatesHaveUniqueIdsAndAreAllVisible()
    {
        var creations = Enumerable.Range(1, 12).Select(async index =>
        {
            using var response = await _client.PostAsJsonAsync(
                "/books", new { title = $"Imaginary Volume {index}", author = "Tessa Reed" });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var book = await response.Content.ReadFromJsonAsync<Book>();
            Assert.NotNull(book);
            return book;
        });

        var created = await Task.WhenAll(creations);
        Assert.Equal(Enumerable.Range(4, 12), created.Select(book => book.Id).Order());
        Assert.Equal(
            ExpectedSeed.Concat(created).OrderBy(book => book.Id).ToArray(),
            await _client.GetFromJsonAsync<Book[]>("/books"));
    }

    private static async Task AssertValidationProblem(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(1, problem.RootElement.GetProperty("errors").GetProperty(field).GetArrayLength());
    }

    private async Task AssertSeedUnchanged()
    {
        Assert.Equal(ExpectedSeed, await _client.GetFromJsonAsync<Book[]>("/books"));
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
