using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoBot
{
    public static class DiscordExtensions
    {
        private static Bot? _bot;

        public static DiscordClient CreateBot(this DiscordClient client)
        {
            _bot = new Bot();
            _bot.Init(client);

            return client;
        }
    }
}
