using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ShopSystem.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string ipAddress = configuration["IpAddress"];
            string portStr = configuration["Port"];
            if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrEmpty(portStr))
            {
                Console.WriteLine("Error: Required configuration values are missing in appsettings.json.");
                return;
            }

            int port;
            if (!int.TryParse(portStr, out port))
            {
                Console.WriteLine("Error: Invalid Port value in appsettings.json.");
                return;
            }

            Console.WriteLine("TCP Client - Connecting to Server");

            try
            {
                TcpClient client = new TcpClient(ipAddress, port);
                NetworkStream stream = client.GetStream();
                Console.WriteLine("Connecting to Server...");

                string response = ReceiveMessage(stream);
                Console.WriteLine("Message from Server: " + response);

                if (response != "Connection successful")
                {
                    Console.WriteLine("Connection failed.");
                    client.Close();
                    return;
                }

                while (true)
                {
                    Console.WriteLine("\nAvailable commands:");
                    Console.WriteLine("- get_products: Get product list");
                    Console.WriteLine("- create_order: Create order (enter JSON items)");
                    Console.WriteLine("- get_order: Get order details (enter OrderId)");
                    Console.WriteLine("- exit: Exit connection");
                    Console.Write("Enter command: ");
                    string command = Console.ReadLine().ToLower();

                    if (command == "exit")
                    {
                        SendMessage(stream, "exit");
                        response = ReceiveMessage(stream);
                        Console.WriteLine("Message from Server: " + response);
                        break;
                    }
                    else if (command == "get_products")
                    {
                        SendMessage(stream, "get_products");
                        response = ReceiveMessage(stream);

                        try
                        {
                            using JsonDocument doc = JsonDocument.Parse(response);
                            if (doc.RootElement.ValueKind == JsonValueKind.Array)
                            {
                                Console.WriteLine("Product list:");
                                foreach (var product in doc.RootElement.EnumerateArray())
                                {
                                    Console.WriteLine($"ProductId: {product.GetProperty("ProductId")}, ProductName: {product.GetProperty("ProductName")}, Quantity: {product.GetProperty("Quantity")}, Price: {product.GetProperty("Price")}");
                                }
                            }
                            else
                            {
                                Console.WriteLine(response);
                            }
                        }
                        catch
                        {
                            Console.WriteLine(response);
                        }
                    }
                    else if (command == "create_order")
                    {
                        string jsonInput = "{\"items\":[{\"ProductId\":1,\"Quantity\":2},{\"ProductId\":3,\"Quantity\":1}]}";

                        try
                        {
                            JsonDocument.Parse(jsonInput);
                        }
                        catch
                        {
                            Console.WriteLine("Invalid JSON. Command canceled.");
                            continue;
                        }

                        SendMessage(stream, jsonInput);
                        response = ReceiveMessage(stream);
                        Console.WriteLine("Response from Server: " + response);
                    }
                    else if (command == "get_order")
                    {
                        Console.Write("Enter OrderId: ");
                        string orderIdInput = Console.ReadLine();
                        if (!int.TryParse(orderIdInput, out int orderId))
                        {
                            Console.WriteLine("Invalid OrderId.");
                            continue;
                        }

                        SendMessage(stream, orderIdInput);
                        response = ReceiveMessage(stream);

                        try
                        {
                            using JsonDocument doc = JsonDocument.Parse(response);
                            if (doc.RootElement.ValueKind == JsonValueKind.Object)
                            {
                                Console.WriteLine($"OrderId: {doc.RootElement.GetProperty("OrderId")}, OrderDate: {doc.RootElement.GetProperty("OrderDate")}, TotalPrice: {doc.RootElement.GetProperty("TotalPrice")}");
                                Console.WriteLine("Product details:");
                                var details = doc.RootElement.GetProperty("OrderDetails");
                                if (details.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var detail in details.EnumerateArray())
                                    {
                                        Console.WriteLine($"ProductName: {detail.GetProperty("ProductName")}, Quantity: {detail.GetProperty("Quantity")}, Price: {detail.GetProperty("Price")}");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine(response);
                            }
                        }
                        catch
                        {
                            Console.WriteLine(response);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid command.");
                    }
                }

                client.Close();
                Console.WriteLine("Connection closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void SendMessage(NetworkStream stream, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        static string ReceiveMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[4096];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
    }
}