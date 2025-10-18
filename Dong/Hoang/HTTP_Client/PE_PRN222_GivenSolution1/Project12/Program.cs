using Microsoft.Extensions.Configuration;
using Project12;
using System.Net.Http.Json;

var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

string ipAddress = config["IpAddress"]!;
string port = config["Port"]!;
string baseUrl = $"{ipAddress}{port}/";

HttpClient client = new HttpClient
{
    BaseAddress = new Uri(baseUrl)
};

while (true)
{
    Console.Write("Please enter integer number (Genre ID): ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Exiting client application...");
        break;
    }

    if (!int.TryParse(input, out int genreId))
    {
        Console.WriteLine("Invalid input! Please enter a valid integer.");
        continue;
    }

    try
    {
        HttpResponseMessage response = await client.GetAsync($"{baseUrl}books/{genreId}");
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Server is not running. Please try again later.");
            continue;
        }

        var books  = await response.Content.ReadFromJsonAsync<List<Book>>();
        if (books != null)
        {
            foreach (var book in books)
            {
                Console.WriteLine(Utils.Stringify(book));
            } 
        }
        Console.WriteLine("--------------------------------------\n");

    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine("Server is not running. Please try again later.");
    }
}