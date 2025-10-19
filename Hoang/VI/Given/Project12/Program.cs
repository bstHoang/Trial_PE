using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Project12.Models; // Đảm bảo bạn đã tạo các model bên dưới

namespace Project12
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var portStr = configuration["Port"];
            var ipAddress = configuration["IpAddress"];
            int port = int.Parse(portStr);

            // Tùy chọn JsonSerializer để không phân biệt hoa/thường khi deserializ
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            while (true)
            {
                Console.Write("Enter MovieID to borrow (or press Enter to exit): ");
                string input = Console.ReadLine();

                // Yêu cầu 1: Nếu input trống, kết thúc chương trình
                if (string.IsNullOrEmpty(input))
                {
                    break;
                }

                // Yêu cầu 2: Validate input
                if (!int.TryParse(input, out int movieId) )
                {
                    Console.WriteLine("Invalid movie ID, please enter an integer");
                    continue; // Quay lại đầu vòng lặp
                }
                if (int.TryParse(input, out int movieId1) && movieId1 <= 0)
                {
                    Console.WriteLine("Invalid movie ID, please enter an integer greater than 0");
                    continue; // Quay lại đầu vòng lặp
                }

                try
                {
                    // Yêu cầu 3: Kết nối tới server
                    using var client = new TcpClient();
                    client.Connect(ipAddress, port); // Kết nối đến server

                    using var stream = client.GetStream();

                    // Tạo và gửi yêu cầu JSON
                    var request = new { MovieId = movieId };
                    string jsonRequest = JsonSerializer.Serialize(request);
                    byte[] requestBytes = Encoding.UTF8.GetBytes(jsonRequest);
                    stream.Write(requestBytes, 0, requestBytes.Length);

                    // Nhận phản hồi
                    var buffer = new byte[4096]; // Bộ đệm lớn để nhận JSON
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Xử lý phản hồi
                    var response = JsonSerializer.Deserialize<BorrowResponse>(jsonResponse, jsonOptions);

                    if (response?.Status == "Success" && response.Movie != null)
                    {
                        // Sử dụng Utils.cs để in thông tin thành công
                        Utils.FormatObject(response.Movie);
                    }
                    else
                    {
                        // In thông báo lỗi từ server
                        Console.WriteLine($"Error: {response?.Message ?? "Unknown server error."}");
                    }
                }
                catch (SocketException)
                {
                    // Yêu cầu 3 (lỗi): Xử lý lỗi kết nối và không đóng client
                    Console.WriteLine("Error connecting to server");
                }
                catch (JsonException)
                {
                    Console.WriteLine("Error: Invalid response format from server.");
                }
                catch (Exception ex)
                {
                    // Xử lý các lỗi ngoại lệ khác
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                }
            }
        }
    }
}