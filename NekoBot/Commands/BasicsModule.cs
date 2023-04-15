using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NekoBot.Commands
{
    public class BasicsModule : BaseCommandModule
    {
        [Command("createinvite")]
        public async Task CreateInvite1HourCommand(CommandContext ctx)
        {
            try
            {
                DiscordInvite? invite = null;

                foreach (KeyValuePair<ulong, DiscordChannel> channels in ctx.Guild.Channels)
                {
                    if (channels.Value.Name.ToLower().StartsWith("role-selection"))
                    {
                        Debug.WriteLine("creating invite");
                        invite = await channels.Value.CreateInviteAsync(3600);
                        continue;
                    }
                }

                if (invite != null)
                {
                    await ctx.Channel.SendMessageAsync($"Link expires after 1 hour: {invite}");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync($"Failed to create invite.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception caught for !invite command\n{ex}");
            }
        }

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
