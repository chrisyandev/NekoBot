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
        public static List<DiscordChannel> autoMuteChannels = new();

        [Command("automutevc"), Aliases("amvc")]
        public async Task AutoMuteCommand(CommandContext ctx, params DiscordMember[] membersUnMuted)
        {
            DiscordChannel voiceChannel = await ctx.Guild.CreateVoiceChannelAsync($"Group {autoMuteChannels.Count} (auto-mute)", ctx.Channel.Parent);
            autoMuteChannels.RemoveAll(x => x == null);
            autoMuteChannels.Add(voiceChannel);

            ctx.Client.VoiceStateUpdated += async (client, e) =>
            {
                var member = e.User as DiscordMember;

                if (member != null)
                {
                    bool joinedAnotherChannel = (e.Before == null || e.Before.Channel == null || e.Before.Channel == voiceChannel)
                                                && (e.After != null && e.After.Channel != null && e.After.Channel != voiceChannel);
                    bool joinedThisChannel = (e.Before == null || e.Before.Channel == null || e.Before.Channel != voiceChannel)
                                            && (e.After != null && e.After.Channel != null && e.After.Channel == voiceChannel);

                    if (member.IsMuted && joinedAnotherChannel && !autoMuteChannels.Contains(e.After!.Channel!))
                    {
                        Debug.WriteLine("unmuting member");
                        await member.SetMuteAsync(false);
                        return;
                    }
                    
                    if (joinedThisChannel)
                    {
                        bool canMemberSpeak = membersUnMuted.Contains(member);

                        if (canMemberSpeak)
                        {
                            Debug.WriteLine("unmuting member");
                            await member.SetMuteAsync(false);
                        }
                        else
                        {
                            Debug.WriteLine("muting member");
                            await member.SetMuteAsync(true);
                        }
                    }
                }
            };
        }

/*        [Command("deletecategorychannels")]
        public async Task DeleteCategoryChannelsCommand(CommandContext ctx)
        {
            foreach (var c in ctx.Channel.Parent.Children)
            {
                await c.DeleteAsync();
            }
        }*/
    }
}
