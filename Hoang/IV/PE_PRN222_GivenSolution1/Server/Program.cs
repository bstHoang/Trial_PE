using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Server.Models; // Đảm bảo using đúng namespace
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

// Lớp đại diện cho cấu trúc JSON của yêu cầu tạo đơn hàng
public class CreateOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
}

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {
        // Tải cấu hình từ appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var ipAddressStr = configuration["IpAddress"];
        var portStr = configuration["Port"];

        if (string.IsNullOrEmpty(ipAddressStr) || !int.TryParse(portStr, out int port))
        {
            Console.WriteLine("Lỗi: Cấu hình không hợp lệ trong appsettings.json.");
            return;
        }

        var ipAddress = IPAddress.Parse(ipAddressStr);
        var listener = new TcpListener(ipAddress, port);
        listener.Start();
        Console.WriteLine($"Server đã khởi động tại {ipAddress}:{port}. Đang chờ kết nối...");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            // Xử lý mỗi client trên một luồng riêng để không chặn các kết nối khác
            _ = Task.Run(() => HandleClient(client));
        }
    }

    static async Task HandleClient(TcpClient client)
    {
        Console.WriteLine("Đã kết nối với client thành công");
        var stream = client.GetStream();
        var reader = new StreamReader(stream, Encoding.UTF8);
        var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        try
        {
            // Gửi thông báo kết nối thành công tới client
            await writer.WriteLineAsync("Kết nối thành công");

            while (client.Connected)
            {
                var commandJson = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(commandJson)) continue; // Ngắt kết nối nếu client đóng stream

                var parts = commandJson.Split(new[] { ' ' }, 2);
                string command = parts[0].ToLower();
                string payload = parts.Length > 1 ? parts[1] : string.Empty;

                Console.WriteLine($"Đã nhận lệnh từ client: {command}");

                switch (command)
                {
                    case "get_products":
                        await HandleGetProducts(writer);
                        break;
                    case "create_order":
                        await HandleCreateOrder(writer, payload);
                        break;
                    default:
                        await writer.WriteLineAsync("Lệnh không hợp lệ.");
                        break;
                }
            }
        }
        catch (IOException)
        {
            Console.WriteLine("Client đã ngắt kết nối.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    private static async Task HandleGetProducts(StreamWriter writer)
    {
        using var context = new ShopDbContext();
        var products = await context.Products.AsNoTracking().ToListAsync();

        if (products.Any())
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonResponse = JsonSerializer.Serialize(products, options);
            await writer.WriteLineAsync(jsonResponse);
        }
        else
        {
            await writer.WriteLineAsync("Hiện không có sản phẩm nào");
        }
    }

    private static async Task HandleCreateOrder(StreamWriter writer, string jsonPayload)
    {
        if (string.IsNullOrWhiteSpace(jsonPayload))
        {
            await writer.WriteLineAsync("Lỗi: Dữ liệu đơn hàng rỗng.");
            return;
        }

        CreateOrderRequest orderRequest;
        try
        {
            orderRequest = JsonSerializer.Deserialize<CreateOrderRequest>(jsonPayload);
        }
        catch (JsonException)
        {
            await writer.WriteLineAsync("Lỗi: Định dạng JSON không hợp lệ.");
            return;
        }

        if (orderRequest == null || !orderRequest.Items.Any())
        {
            await writer.WriteLineAsync("Lỗi: Đơn hàng không có sản phẩm.");
            return;
        }

        using var context = new ShopDbContext();
        // Bắt đầu một transaction để đảm bảo tất cả các thao tác đều thành công hoặc thất bại cùng nhau
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            decimal totalAmount = 0;

            // Lấy ID sản phẩm từ request để truy vấn một lần
            var requestedProductIds = orderRequest.Items.Select(i => i.ProductId).ToList();
            var productsInDb = await context.Products
                .Where(p => requestedProductIds.Contains(p.ProductId))
                .ToListAsync();

            var newOrderDetails = new List<OrderDetail>();

            foreach (var item in orderRequest.Items)
            {
                if (item.Quantity <= 0)
                {
                    await transaction.RollbackAsync();
                    await writer.WriteLineAsync("Số lượng không phù hợp.");
                    return;
                }

                var product = productsInDb.FirstOrDefault(p => p.ProductId == item.ProductId);

                if (product == null)
                {
                    await transaction.RollbackAsync();
                    await writer.WriteLineAsync($"Lỗi: Không tìm thấy sản phẩm với ID: {item.ProductId}.");
                    return;
                }

                if (product.Quantity < item.Quantity)
                {
                    await transaction.RollbackAsync();
                    await writer.WriteLineAsync($"Số lượng trong kho không đủ cho sản phẩm '{product.ProductName}'. Tồn kho: {product.Quantity}.");
                    return;
                }

                // Trừ số lượng tồn kho
                product.Quantity -= item.Quantity;

                // Tính tổng giá trị cho sản phẩm này
                decimal itemTotalPrice = item.Quantity * product.Price;
                totalAmount += itemTotalPrice;

                // Thêm chi tiết đơn hàng vào danh sách chờ
                newOrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price // Lấy giá từ DB để đảm bảo chính xác
                });
            }

            // Tạo đơn hàng mới
            var newOrder = new Order
            {
                OrderDate = DateOnly.FromDateTime(DateTime.Now),
                TotalPrice = totalAmount,
                OrderDetails = newOrderDetails // Gán danh sách chi tiết
            };

            context.Orders.Add(newOrder);

            // Lưu tất cả thay đổi (tạo Order, OrderDetail, cập nhật Product)
            await context.SaveChangesAsync();

            // Nếu mọi thứ thành công, commit transaction
            await transaction.CommitAsync();

            await writer.WriteLineAsync($"Tạo đơn hàng thành công! Mã đơn hàng: {newOrder.OrderId}, Tổng giá trị: {totalAmount:N0} VND");
        }
        catch (Exception ex)
        {
            // Nếu có lỗi, rollback transaction
            await transaction.RollbackAsync();
            Console.WriteLine($"Lỗi khi tạo đơn hàng: {ex.ToString()}");
            await writer.WriteLineAsync("Đã xảy ra lỗi phía server khi xử lý đơn hàng.");
        }
    }
}

