using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Server.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mime;
using System.Web;

namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IConfigurationRoot configuration = LoadAppSetting();

            string? serverRoot = $"{configuration["IPAddress"]}:{configuration["Port"]}/";

            if (serverRoot == null)
            {
                throw new ArgumentNullException(nameof(serverRoot));
            }

            HttpListener listener = InitServer(serverRoot);

            while (true)
            {
                LibraryContext libraryContext = new LibraryContext();

                var context = await listener.GetContextAsync();
                var request = context.Request;
                var response = context.Response;
                var path = request.Url.AbsolutePath.ToLower();
                var method = request.HttpMethod;

                if (method == "GET" || path == "/list")
                {
                    var listBook = await libraryContext.Books
                        .Select(b => new
                        {
                            b.BookId,
                            b.Title,
                            b.PublicationYear,
                            Genres = b.Genre.GenreName,
                            Authors = b.Authors.Select(au => new
                            {
                                au.Name,
                                au.BirthYear
                            })
                        })
                        .ToListAsync();

                    if (listBook == null) 
                    {
                        await Helper.WriteJsonResponse(response, new List<Book>(), MediaTypeNames.Application.Json, HttpStatusCode.OK);
                    }
                    else
                    {
                        await Helper.WriteJsonResponse(response, listBook, MediaTypeNames.Application.Json, HttpStatusCode.OK);
                    }
                    continue;
                }

                if (method == "DELETE" && path == "/delete")
                {
                    System.Collections.Specialized.NameValueCollection query = System.Web.HttpUtility.ParseQueryString(request.Url.Query);
                    if (!int.TryParse(query["id"], out int id))
                    {
                        await Helper.WriteJsonResponse(response, "Invalid ID format.", MediaTypeNames.Application.Json, HttpStatusCode.BadRequest);
                        return;
                    }
                    else
                    {
                        Book? book = libraryContext.Books
                                    .Include(b => b.Authors)
                                    .Include(b => b.Genre)
                                    .FirstOrDefault(b => b.BookId == id);
                        if (book == null)
                        {
                            await Helper.WriteJsonResponse(response, "Book does not exist!", MediaTypeNames.Application.Json, HttpStatusCode.NotFound);
                        }
                        else
                        {
                            libraryContext.Books.Remove(book);
                            await libraryContext.SaveChangesAsync();
                            await Helper.WriteJsonResponse(response, "Deleted.", MediaTypeNames.Application.Json, HttpStatusCode.OK);
                        }
                    }
                }
            }
        }

        private static HttpListener InitServer(string serverRoot)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(serverRoot);
            listener.Start();
            return listener;
        }

        private static IConfigurationRoot LoadAppSetting()
        {
            return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        }
    }
}