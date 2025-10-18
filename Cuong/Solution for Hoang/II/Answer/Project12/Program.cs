using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Net;

// ===========================
// Data Transfer Objects (DTOs)
// ===========================

// Dùng để chứa dữ liệu sách từ server
public class BookDto
{
    public int BookId { get; set; }
    public string Title { get; set; }
    public int PublicationYear { get; set; }
    public string GenreName { get; set; }
}

// Dùng để chứa các tin nhắn từ server (cả lỗi và thành công)
public class ServerMessage
{
    public string message { get; set; }
}

// ===========================
// Main Program
// ===========================

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static string baseUrl = "";

    static async Task Main(string[] args)
    {
        // Đọc cấu hình từ appsettings.json
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string ipAddress = config["IpAddress"];
        string port = config["Port"];
        baseUrl = $"{ipAddress}{port}/";

        Console.WriteLine($"Client started, connecting to Server at {baseUrl}");

        bool isRunning = true;
        while (isRunning)
        {
            // Hiển thị menu
            Console.WriteLine("\n---------------- MENU ----------------");
            Console.WriteLine("1. List books by Genre ID");
            Console.WriteLine("2. Delete book by ID");
            Console.WriteLine("Enter any other key to exit.");
            Console.WriteLine("--------------------------------------");
            Console.Write("Choose an option: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await ListBooksByGenreAsync();
                    break;
                case "2":
                    await DeleteBookByIdAsync();
                    break;
                default:
                    isRunning = false; // Thoát khỏi vòng lặp
                    break;
            }
        }
        Console.WriteLine("Client application has been closed.");
    }

    // --- Chức năng 1: Lấy danh sách sách theo Genre ID ---
    private static async Task ListBooksByGenreAsync()
    {
        Console.Write("Enter Genre ID: ");
        string genreId = Console.ReadLine();

        try
        {
            var response = await client.GetAsync($"{baseUrl}books/{genreId}");
            string jsonResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Deserialize thành danh sách BookDto
                var books = JsonSerializer.Deserialize<List<BookDto>>(jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Dựa vào code server, nếu không tìm thấy, server sẽ trả về mảng rỗng []
                if (books != null && books.Any())
                {
                    Console.WriteLine("\n--- Books Found ---");
                    foreach (var book in books)
                    {
                        Console.WriteLine($"  ID: {book.BookId}, Title: '{book.Title}', Year: {book.PublicationYear}, Genre: {book.GenreName}");
                    }
                }
                else
                {
                    Console.WriteLine($"\n--- No books found for Genre ID {genreId} ---");
                }
            }
            else
            {
                // Xử lý các lỗi khác, ví dụ ID không hợp lệ
                Console.WriteLine($"\n--- Server returned an error: {response.StatusCode} ---");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR]: An error occurred: {ex.Message}");
        }
    }

    // --- Chức năng 2: Xóa sách theo Book ID ---
    private static async Task DeleteBookByIdAsync()
    {
        Console.Write("Enter Book ID to delete: ");
        string bookId = Console.ReadLine();

        try
        {
            var response = await client.DeleteAsync($"{baseUrl}books/{bookId}");

            // Xử lý các trường hợp dựa trên StatusCode
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK: // 200 - Thành công
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var successMessage = JsonSerializer.Deserialize<ServerMessage>(jsonResponse,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Console.WriteLine($"\n--- SUCCESS: {successMessage?.message} ---");
                    break;

                case HttpStatusCode.NotFound: // 404 - Không tìm thấy
                    Console.WriteLine($"\n--- ERROR: Book with ID {bookId} not found. ---");
                    break;

                case HttpStatusCode.BadRequest: // 400 - ID không hợp lệ
                    Console.WriteLine("\n--- ERROR: The ID provided was invalid. Please enter an integer. ---");
                    break;

                default: // Các lỗi khác
                    Console.WriteLine($"\n--- An unexpected error occurred. Status: {response.StatusCode} ---");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR]: An error occurred: {ex.Message}");
        }
    }
}