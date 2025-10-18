using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Project11.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;

namespace TCP_Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Cấu hình để đọc ConnectionString
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();


            // 1. Khởi tạo Server
            var portStr = configuration["Port"];
            var ipAddressStr = configuration["IpAddress"];

            int port = int.Parse(portStr);
            System.Net.IPAddress ipAddress = System.Net.IPAddress.Parse(ipAddressStr);

            var listener = new System.Net.Sockets.TcpListener(ipAddress, port);
            listener.Start();

            // Yêu cầu 4.2.1: Hiển thị thông tin server
            Console.WriteLine($"Server is running at {ipAddressStr}:{portStr}");
            Console.WriteLine("Waiting for client connections...");

            while (true)
            {
                // Chấp nhận kết nối từ client
                var client = await listener.AcceptTcpClientAsync();
                
            }
        }

        static async Task HandleClientAsync(TcpClient client, DbContextOptions<TcpServerClientContext> dbOptions)
        {
            try
            {
                using var stream = client.GetStream();
                var buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string receivedName = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Yêu cầu 4.2.2: Hiển thị request từ client
                Console.WriteLine($"Client sends a request: {receivedName}");

                // Yêu cầu 4.2.3: Truy vấn database
                await using var context = new TcpServerClientContext(dbOptions);

                // Truy vấn User, bao gồm Orders, OrderDetails, và Product
                var user = await context.Users
                    .Include(u => u.Orders)
                        .ThenInclude(o => o.OrderDetails)
                            .ThenInclude(od => od.Product)
                    .SingleOrDefaultAsync(u => u.FullName.Equals(receivedName));

                string xmlResponse = "";

                if (user != null)
                {
                    // Map từ EF Model sang DTO
                    var userDto = new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Address = user.Address ?? "N/A",
                        Gender = MapGender(user.Gender),
                        Orders = user.Orders.Select(o => new OrderDto
                        {
                            Id = o.Id,
                            OrderDate = o.OrderDate.ToString("dd/MM/yyyy"),
                            Status = MapStatus(o.Status),
                            OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto
                            {
                                ProductName = od.Product.Name,
                                Quantity = od.Quantity
                            }).ToList()
                        }).ToList()
                    };

                    // Đóng gói dữ liệu sang XML
                    var serializer = new XmlSerializer(typeof(UserDto));
                    using var stringWriter = new StringWriter();
                    serializer.Serialize(stringWriter, userDto);
                    xmlResponse = stringWriter.ToString();
                }
                // Nếu user == null (không tìm thấy), xmlResponse sẽ là string rỗng ""
                // Client sẽ tự xử lý trường hợp này (Yêu cầu 4.1.4)

                // Gửi phản hồi XML về client
                byte[] responseBytes = Encoding.UTF8.GetBytes(xmlResponse);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        // Helper functions để map int sang string
        private static string MapGender(int? gender)
        {
            return gender switch
            {
                0 => "Nữ",
                1 => "Nam",
                2 => "Khác",
                _ => "Không xác định"
            };
        }

        private static string MapStatus(int? status)
        {
            return status switch
            {
                0 => "Pending",
                1 => "Done",
                2 => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}