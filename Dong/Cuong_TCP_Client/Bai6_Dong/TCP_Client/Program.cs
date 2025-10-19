using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TCP_Client;
using TCP_Client.Models;

IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
string ipAddress = config["IpAddress"]!;
int port = int.Parse(config["Port"]!);

while (true)
{
    Console.Write("Enter Movie ID to borrow (or press Enter to exit):");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        break;
    }
    if (!int.TryParse(input, out int movieId))
    {
        Console.WriteLine("Invalid movie ID, please enter an integer");
        continue;
    }

    if (movieId <= 0)
    {
        Console.WriteLine("Invalid movie ID, please enter an integer greater than 0");
        continue;
    }

    try
    {
        TcpClient tcpClient = new TcpClient(ipAddress, port);
        using var stream = tcpClient.GetStream();
        var request = new { MovieId = movieId };
        string jsonRequest = JsonSerializer.Serialize(request);
        byte[] byteRequest = Encoding.UTF8.GetBytes(jsonRequest);
        stream.Write(byteRequest, 0, byteRequest.Length);

        // response
        byte[] buffer = new byte[4096];
        int byteReader = stream.Read(buffer, 0, buffer.Length);
        if (byteReader == 0)
        {
            Console.WriteLine("Error to connecting to server");
            break;
        }

        string jsonResponse = Encoding.UTF8.GetString(buffer,0,byteReader);
        using var dataParse = JsonDocument.Parse(jsonResponse);
        string status = dataParse.RootElement.GetProperty("Status").GetString()!;
        
        //if(status == "Success")
        //{
        //    var movieJson = dataParse.RootElement.GetProperty("Movie").GetRawText()!;
        //    var movie = JsonSerializer.Deserialize<Movie>(movieJson);

        //    Utils.FormatObject(movie);
        //} else if(status == "Error")
        //{
        //    string message = dataParse.RootElement.GetProperty("Message").GetString()!;
        //    Console.WriteLine(message);
        //}


    }
    catch (Exception ex)
    {
        Console.WriteLine("Error to connecting to server");
    }

}