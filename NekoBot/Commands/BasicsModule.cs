using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;
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
            const string TargetChannelName = "role-selection";
            const string LogChannelName = "member-log";

            try
            {
                DiscordInvite? invite = null;

                var targetChannel = ctx.Guild.Channels.Values.FirstOrDefault(channel => channel.Name.ToLower().Contains(TargetChannelName));
                if (targetChannel != null)
                {
                    Console.WriteLine("creating invite");
                    invite = await targetChannel.CreateInviteAsync(600, 1, false, true, $"The !createinvite command was used by {ctx.User}");
                }

                if (invite != null)
                {
                    await ctx.Channel.SendMessageAsync($"Link expires after 10 minutes (one time use): {invite}");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync($"Failed to create invite. Make sure channel #{TargetChannelName} exists.");
                }
                
                var logChannel = ctx.Guild.Channels.Values.FirstOrDefault(channel => channel.Name.ToLower().Contains(LogChannelName));
                if (logChannel != null)
                {
                    await logChannel.SendMessageAsync($"The !createinvite command was used by {ctx.User}");
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
