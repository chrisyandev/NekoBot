using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoBot.Commands
{
    public class BasicsModule : BaseCommandModule
    {
        //[Command("greet")]
        public async Task GreetCommand(CommandContext ctx, [RemainingText] string name)
        {
            await ctx.RespondAsync($"Greetings {name}!");
        }

        //[Command("greet")]
        public async Task GreetCommand(CommandContext ctx, DiscordUser name)
        {
            await ctx.RespondAsync($"Greetings {name}!");
        }
    }
}
