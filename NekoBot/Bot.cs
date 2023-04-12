using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Configuration;
using NekoBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoBot
{
    public class Bot
    {
        public DiscordClient? Client { get; private set; }
        public InteractivityExtension? Interactivity { get; private set; }
        public CommandsNextExtension? CommandsNext { get; private set; }

        public async Task RunAsync()
        {
            var secrets = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Program).Assembly, true)
                .Build();

            var discordConfig = new DiscordConfiguration()
            {
                Token = secrets["TOKEN"],
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            };
            Client = new DiscordClient(discordConfig);

            var interactConfig = new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(3)
            };
            Interactivity = Client.UseInteractivity(interactConfig);

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" }
            };
            CommandsNext = Client.UseCommandsNext(commandsConfig);
            CommandsNext.RegisterCommands<BasicsModule>();
            CommandsNext.RegisterCommands<ReactionRoleModule>();
            CommandsNext.RegisterCommands<AutoMuteVoiceModule>();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
