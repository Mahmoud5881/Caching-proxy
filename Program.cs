using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Net;
using System.Runtime.InteropServices;

namespace caching_proxy
{
    public class Program
    {

        private static readonly ConcurrentDictionary<string,string> cache = new();
        private static readonly HttpClient client = new();
        public static async Task Main(string[] args)
        {
            int port = 0;
            string? origin = null;
            bool clearCache = false;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--origin" when i + 1 < args.Length:
                        origin = args[i + 1];
                        break;
                    case "--port" when i + 1 < args.Length:
                        port = int.Parse(args[i + 1]);
                        break;
                    case "--clear-cache":
                        clearCache = true;
                        break;
                }
            }

                if (port == 0 || origin == null)
                {
                    Console.WriteLine("Usage: caching-proxy --port <number> --origin <url>");
                    return;
                }

                if (clearCache) { 
                    cache.Clear();
                    Console.WriteLine("Cache Cleared");
                    return;
                }

                string prefix = $"http://localhost:{port}/";
                var listener = new HttpListener();
                listener.Prefixes.Add(prefix);
                listener.Start();

                Console.WriteLine($"Caching proxy server started on {prefix}");
                Console.WriteLine($"Forwarding requests to {origin}");

                while(true)
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequest(context, origin));
                }
        }

        public static async Task HandleRequest(HttpListenerContext context, string origin)
        {
            string path = context.Request.Url.PathAndQuery;
            string url = $"{origin}{path}";

            if (cache.TryGetValue(url, out string cachedResponse))
            {
                context.Response.Headers["X-Cache"] = "HIT";
                Console.WriteLine($"Cache HIT: HIT");
                await WriteResponse(context, cachedResponse);
            }
            else
            {
                context.Response.Headers["X-Cache"] = "MISS";
                Console.WriteLine($"Cache HIT: MISS");

                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(context.Request.HttpMethod), url);
                    HttpResponseMessage response = await client.SendAsync(request);

                    string responseBody = await response.Content.ReadAsStringAsync();

                    cache[url] = responseBody;
                    await WriteResponse(context, responseBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error forwarding request: {ex.Message}");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await WriteResponse(context, "Internal server error");
                }
            }
        }

        private static async Task WriteResponse(HttpListenerContext context, string responseBody)
        {
            context.Response.ContentType = "application/json"; // Assuming JSON responses
            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
            {
                await writer.WriteAsync(responseBody);
            }
            context.Response.Close();
        }   
    }
}
