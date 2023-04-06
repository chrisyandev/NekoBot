using System;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Configuration;

namespace NekoBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Program).Assembly, true)
                .Build();

            var client = new DiscordClient(new DiscordConfiguration()
            {
                Token = config["TOKEN"],
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            client.CreateBot();

            await client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
