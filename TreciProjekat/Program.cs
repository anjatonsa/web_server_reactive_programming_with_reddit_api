using Reddit;
using Reddit.Controllers;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using VaderSharp;
namespace TreciProjekat
{
    internal class Program
    {
        static string clientId = Environment.GetEnvironmentVariable("REDDIT_API_CLIENT_ID");
        static string secret= Environment.GetEnvironmentVariable("REDDIT_API_SECRET");
        static string port = "8080/";
        static string url = "http://localhost:" + port;
        static async Task Main()
        {

            Console.WriteLine("Main thread...");

            string[] arguments = { url, clientId, secret };

            WebServer server = new WebServer(arguments);
            await server.Start();

        }
    }
}


