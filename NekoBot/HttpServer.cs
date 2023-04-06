using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NekoBot
{
    class HttpServer
    {
        public static async void KeepAlive()
        {
            using var listener = new HttpListener();
            //listener.Prefixes.Add("http://*:8000/"); // for hosting on Replit
            listener.Prefixes.Add("http://localhost:8000/");

            listener.Start();

            Console.WriteLine("Listening on port 8000...");

            while (true)
            {
                HttpListenerContext ctx = listener.GetContext();
                using HttpListenerResponse resp = ctx.Response;

                resp.StatusCode = (int)HttpStatusCode.OK;
                resp.StatusDescription = "Status OK";

                resp.Headers.Set("Content-Type", "text/plain");
                string data = "Hello there!";
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                resp.ContentLength64 = buffer.Length;

                using Stream output = resp.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}
