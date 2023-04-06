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

            CreateBot(client);

            await client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static void CreateBot(DiscordClient client)
        {
            var commands = client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" }
            });
            commands.RegisterCommands<BasicsModule>();
        }
    }
}
