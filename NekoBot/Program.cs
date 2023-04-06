using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Configuration;
using NekoBot.Commands;

namespace NekoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot();

            Thread thread = new Thread(HttpServer.KeepAlive);
            thread.Start();

            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
