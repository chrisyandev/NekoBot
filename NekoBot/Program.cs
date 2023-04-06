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

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = config["TOKEN"],
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync("pong!");
            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
