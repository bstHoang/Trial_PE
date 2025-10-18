using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Project11.Model;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

string ipAddress = config["IPAddress"]!;
string port = config["Port"]!;
string serverUrl = $"{ipAddress}{port}/";

var listener = new HttpListener();
listener.Prefixes.Add(serverUrl);
listener.Start();
Console.WriteLine($" Server started at {serverUrl}");
while (true)
{
    try
    {
        var context = await listener.GetContextAsync();
        var request = context.Request;
        var response = context.Response;

        var path = request.Url.AbsolutePath.ToLower()!;
        var method = request.HttpMethod;
        Console.WriteLine($"{method} {path}");
        using var db = new LibraryContext();

        if (path.Contains("/books") && method == "GET")
        {
            var idParam = path.Replace("/books/", "");
            if (int.TryParse(idParam, out var id))
            {
                var books = await db.Books
                    .Include(b => b.Genre)
                    .Where(b => b.GenreId == id)
                    .Select(b => new
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        PublicationYear = b.PublicationYear,
                        GenreName = b.Genre.GenreName
                    })
                    .ToListAsync();

                if (books.Any())
                {
                    var booksJson = JsonSerializer.Serialize(books);
                    var buffer = Encoding.UTF8.GetBytes(booksJson);
                    response.ContentType = "application/json";
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }   
        }
    }
    catch (Exception ex)
    {

    }
}