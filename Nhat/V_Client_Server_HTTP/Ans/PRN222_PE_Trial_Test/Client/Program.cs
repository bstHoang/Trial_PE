using Microsoft.Extensions.Configuration;
using Server;
using System;
using System.ComponentModel.Design;
using System.Net;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IConfigurationRoot configuration = LoadAppSetting();

            HttpClient client = new HttpClient();

            string? serverRoot = $"{configuration["IpAddress"]}:{configuration["Port"]}/";
            ;

            if (serverRoot == null)
            {
                throw new ArgumentNullException(nameof(serverRoot));
            }

            while (true)
            {
                Menu.MainMenu();
                string? option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        await ListBooksAsync(client, serverRoot);
                        break;
                    case "2":
                        await DeleteBookAsync(client, serverRoot);
                        break;
                    case "3":
                        Console.WriteLine("Exited!");
                        return;
                    default:
                        Console.WriteLine("Invalid option, please try again with only integers in 1–3 range.");
                        break;
                }
            }
        }

        private static async Task ListBooksAsync(HttpClient client, string serverRoot)
        {
            try
            {
                var response = await client.GetAsync($"{serverRoot}list");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var books = JsonSerializer.Deserialize<List<BookDTO>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    Console.WriteLine("\n====== Book List ======");
                    if (books != null && books.Count > 0)
                    {
                        foreach (var book in books)
                        {
                            Console.WriteLine(Helper.Stringify(book));
                        }
                    }
                    else
                    {
                        Console.WriteLine("[]");
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        private static async Task DeleteBookAsync(HttpClient client, string serverRoot)
        {
            int id;
            while (true)
            {
                Menu.DeleteMenu();
                if (!int.TryParse(Console.ReadLine(), out id))
                {
                    Console.WriteLine("Invalid ID.");
                }
                else
                {
                    break;
                }
            }

            try
            {
                var response = await client.DeleteAsync($"{serverRoot}delete?id={id}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Deleted.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("Book does not exist!");
                }
                else
                {
                    Console.WriteLine($"Failed to delete book. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
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
