using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
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

            int choice = 0;
            while (choice != 3)
            {
                Console.WriteLine("\n=== Book Store ===");
                Console.WriteLine("1. List Book");
                Console.WriteLine("2. Delete Book");
                Console.WriteLine("3. Exit");
                Console.Write("Option: ");

                string? optionInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(optionInput))
                {
                    Console.WriteLine("Invalid input! Please enter a valid integer.");
                    continue;
                }

                if (!int.TryParse(optionInput, out choice))
                {
                    Console.WriteLine("Invalid input! Please enter a valid integer.");
                    continue;
                }

                if (choice < 1 || choice > 3)
                {
                    Console.WriteLine("Invalid option, please try again with only integers in 1–3 range.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        try
                        {
                            HttpResponseMessage response = await client.GetAsync($"{baseUrl}list");
                            if (response != null)
                            {
                                string jsonRead = await response.Content.ReadAsStringAsync();
                                var books = JsonSerializer.Deserialize<List<BookDto>>(jsonRead);
                                if (books != null)
                                {
                                    foreach (BookDto book in books)
                                    {
                                        Console.WriteLine(Helper.Stringify<BookDto>(book));
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                        }
                        break;

                    case 2:
                        string idInput;
                        int idDelete = 0;
                        while (true)
                        {
                            Console.WriteLine("BookId: ");
                            idInput = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(idInput))
                            {
                                Console.WriteLine("Invalid ID!");
                                continue;
                            }

                            if (!int.TryParse(idInput, out idDelete) && idDelete <= 1)
                            {
                                Console.WriteLine("Invalid ID!");
                                continue;
                            }
                            if (idDelete >= 1) break;
                        }

                        HttpResponseMessage response2 = await client.DeleteAsync($"{baseUrl}delete?id={idDelete}");
                        string content2 = await response2.Content.ReadAsStringAsync();
                        Console.WriteLine(content2);
                        // TODO: Gọi API để xóa book
                        break;

                    case 3:
                        Console.WriteLine("Exited");
                        break;
                }
            }
        }
    }
}
