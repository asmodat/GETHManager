using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;

namespace GEthManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private const int defaultPort = 8000;

        public static IWebHost BuildWebHost(string[] args)
        {
            var port = Environment.GetEnvironmentVariable("PORT").ToIntOrDefault(defaultPort);

            Console.WriteLine($"Starting GETHManager Server on PORT {port}");

           return WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseKestrel(options =>
            {
                options.Listen(IPAddress.Any, port);
            }).ConfigureLogging((context, logging) =>
            {
                logging.AddConsole();
                logging.AddDebug();
            })
            .Build();
        }
    }
}
