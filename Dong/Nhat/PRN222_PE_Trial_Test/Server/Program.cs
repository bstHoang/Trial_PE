using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Server.Model;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // All package are given AS-IT, add or removal of any package is prohibited and your exam will get ZERO
            var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

            string ipAddress = config["IPAddress"];
            string port = config["Port"];
            string serverUrl = $"{ipAddress}{port}/";

            // 2. Khởi tạo HttpListener
            var listener = new HttpListener();
            listener.Prefixes.Add(serverUrl);
            listener.Start();
            using var dbContext = new LibraryContext();
            while (true)
            {
                var context = await listener.GetContextAsync();
                var request = context.Request;
                var response = context.Response;

                var path = request.Url.AbsolutePath.ToLower();
                var method = request.HttpMethod;
                if (path == "/list" && method == "GET")
                {
                    var books = dbContext.Books.Include(b => b.Authors)
                        .Select(b => new
                        {
                            b.BookId,
                            b.Title,
                            b.PublicationYear,
                            Genres = b.Genre.GenreName,
                            Authors = b.Authors.Select(a => new
                            {
                                a.Name,
                                a.BirthYear
                            }),
                        })
                        .ToList();
                    var jsonData = JsonSerializer.Serialize(books);
                    var buffer = Encoding.UTF8.GetBytes(jsonData);
                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }

                if (path == "/delete" && method == "DELETE")
                {
                    var paramId = request.QueryString["id"];
                    if (paramId != null && int.TryParse(paramId, out var idDelete))
                    {
                        var bookDelete = dbContext.Books
                            .Include(b  => b.Authors)
                            .Include(b => b.BookCopies)
                            .FirstOrDefault(b => b.BookId == idDelete);
                        if (bookDelete != null)
                        {
                            foreach(var copy in bookDelete.BookCopies)
                            {
                                dbContext.BorrowHistories.RemoveRange(copy.BorrowHistories);
                            }
                            dbContext.Books.Remove(bookDelete);
                            dbContext.SaveChanges();
                            response.StatusCode = 200;
                            using var writer = new StreamWriter(response.OutputStream);
                            writer.Write($"Deleted");
                        }
                        else
                        {
                            response.StatusCode = 404;
                            using (var writer = new StreamWriter(response.OutputStream)) {
                                writer.WriteLine("Book does not exist!");
                            };
                        }
                    }
                }
            }

        }
    }
}