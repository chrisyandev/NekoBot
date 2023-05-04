using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace NekoBot.Commands
{
    public class StageChannelsModule : BaseCommandModule
    {
        [Command("createstage")]
        public async Task CreateStageCommand(CommandContext ctx)
        {
            DiscordChannel? stageChannel = null;

            try
            {
                Console.WriteLine("Creating Stage");
                stageChannel = await ctx.Guild.CreateChannelAsync($"{ctx.Member?.DisplayName}'s Stage", ChannelType.Stage, ctx.Channel.Parent);

                DiscordOverwriteBuilder permsOverwrite = new DiscordOverwriteBuilder(ctx.Member);
                permsOverwrite.Allow(Permissions.ManageChannels | Permissions.MuteMembers | Permissions.MoveMembers);
                await stageChannel.ModifyAsync(x => x.PermissionOverwrites = new DiscordOverwriteBuilder[] { permsOverwrite });

                ctx.Client.StageInstanceDeleted += Client_StageInstanceDeleted;
                ctx.Client.ChannelDeleted += Client_ChannelDeleted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught for !createstage command\n{ex}");
            }

            async Task Client_StageInstanceDeleted(DiscordClient sender, StageInstanceDeleteEventArgs e)
            {
                try
                {
                    Console.WriteLine($"Deleting Stage channel: {e.Channel}");
                    await e.Channel.DeleteAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception caught when deleting Stage channel\n{ex}");
                }
            }

            Task Client_ChannelDeleted(DiscordClient client, ChannelDeleteEventArgs e)
            {
                Console.WriteLine($"Stage channel deleted: {e.Channel}");

                if (e.Channel == stageChannel)
                {
                    client.StageInstanceDeleted -= Client_StageInstanceDeleted;
                    client.ChannelDeleted -= Client_ChannelDeleted;
                }

                return Task.CompletedTask;
            }
        }
    }
}
