using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Project12.Models;

namespace Project12
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var portStr = configuration["Port"];
            var ipAddress = configuration["IpAddress"];
            int port = int.Parse(portStr);
        }
    }
}