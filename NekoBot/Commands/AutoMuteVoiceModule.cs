using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoBot.Commands
{
    public class AutoMuteVoiceModule : BaseCommandModule
    {
        [Command("automutevc"), Aliases("amvc")]
        public async Task AutoMuteCommand(CommandContext ctx, int numUnMuted)
        {
            /*Debug.WriteLine(numUnMuted);

            DiscordChannel voiceChannel = await ctx.Guild.CreateVoiceChannelAsync("TEST (auto-mute)", ctx.Channel.Parent);

            Debug.WriteLine(voiceChannel);

            ctx.Client.VoiceStateUpdated += async (client, e) =>
            {
                if (e.Channel != voiceChannel)
                {
                    return;
                }

                if (voiceChannel.Users.Count > numUnMuted)
                {
                    e.User.
                }
            };*/

        }

        [Command("automutevc")]
        public async Task AutoMuteCommand(CommandContext ctx, params DiscordMember[] membersUnMuted)
        {
            foreach (DiscordMember mem in membersUnMuted)
            {
                Debug.WriteLine(mem);
            }

            DiscordChannel voiceChannel = await ctx.Guild.CreateVoiceChannelAsync("TEST (auto-mute)", ctx.Channel.Parent);

            Debug.WriteLine(voiceChannel);

            ctx.Client.VoiceStateUpdated += async (client, e) =>
            {
                var member = e.User as DiscordMember;

                if (member != null)
                {
                    bool joinedThisChannel = (e.Before == null || e.Before.Channel != voiceChannel)
                                            && (e.After != null && e.After.Channel == voiceChannel);
                    bool leftThisChannel = (e.Before != null && e.Before.Channel == voiceChannel)
                                            && (e.After == null || e.After.Channel != voiceChannel);

                    if (joinedThisChannel && !membersUnMuted.Contains(member))
                    {
                        Debug.WriteLine("joined channel");
                        await member.SetMuteAsync(true);
                    }
                    else if (leftThisChannel)
                    {
                        Debug.WriteLine("left channel");
                        await member.SetMuteAsync(false);
                    }
                }
            };
        }
    }
}
