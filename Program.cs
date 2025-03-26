using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://*:8080/");
        listener.Start();
        Console.WriteLine("Proxy Server running on port 8080...");

        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            string targetUrl = context.Request.QueryString["target"];

            if (string.IsNullOrEmpty(targetUrl))
            {
                context.Response.StatusCode = 400;
                byte[] error = System.Text.Encoding.UTF8.GetBytes("Error: No target URL provided.");
                await context.Response.OutputStream.WriteAsync(error, 0, error.Length);
                context.Response.Close();
                continue;
            }

            try
            {
                using HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(targetUrl);
                byte[] content = await response.Content.ReadAsByteArrayAsync();

                context.Response.StatusCode = (int)response.StatusCode;
                context.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "text/plain";
                await context.Response.OutputStream.WriteAsync(content, 0, content.Length);
            }
            catch (Exception ex)
            {
                byte[] error = System.Text.Encoding.UTF8.GetBytes($"Proxy Error: {ex.Message}");
                await context.Response.OutputStream.WriteAsync(error, 0, error.Length);
            }

            context.Response.Close();
        }
    }
}
