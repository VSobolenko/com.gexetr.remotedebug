﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LocalHttpServer
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await Run();
            }
            catch (Exception e)
            {
                Console.WriteLine("server error: " + e.Message);
                Console.WriteLine("stack: " + e.StackTrace);
                Console.ReadKey();
            }
        }

        private static async Task Run()
        {
            const int port = 8009;
            var url = GetUrl("*", port);
            var listener = new HttpListener();
            listener.Prefixes.Add(url);

            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string logFilePath = Path.Combine(exeDirectory, "logger.log");

            Console.WriteLine("Log file path: " + logFilePath);
            Console.WriteLine();

            listener.Start();

            var localIp = GetLocalIpAddress();

            Console.WriteLine($"Server started!");
            Console.WriteLine($"URL: {GetUrl(localIp, port)}");
            Console.WriteLine($"IP: {localIp}");
            Console.WriteLine($"Port: {port}");

            while (true)
            {
                var context = await listener.GetContextAsync();
                var request = context.Request;
                var response = context.Response;

                string receivedText = string.Empty;
                if (request.HasEntityBody)
                {
                    using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
                    {
                        receivedText = await reader.ReadToEndAsync();
                    }
                }

                string decodedText = WebUtility.HtmlDecode(receivedText);
                Console.WriteLine($"[LOG]{decodedText}");

                using (var writer = new StreamWriter(logFilePath, append: true, encoding: Encoding.UTF8))
                {
                    await writer.WriteLineAsync(decodedText);
                }

                string responseMessage = $"Response NiceJob!";
                byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);

                response.ContentType = "text/plain";
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
        }

        private static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        private static string GetUrl(string ip, int port) => $"http://{ip}:{port}/";
    }
}
