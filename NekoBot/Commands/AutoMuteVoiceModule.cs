﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using NekoBot.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NekoBot.Commands
{
    public class AutoMuteVoiceModule : BaseCommandModule
    {
        public static List<DiscordChannel> autoMuteChannels = new();

        [Command("automutevc"), Aliases("amvc")]
        public async Task AutoMuteCommand(CommandContext ctx, params DiscordMember[] membersUnMuted)
        {
            DiscordChannel voiceChannel = await ctx.Guild.CreateVoiceChannelAsync($"Team {ctx.User.Username} (auto-mute)", ctx.Channel.Parent);
            autoMuteChannels.Add(voiceChannel);

            ctx.Client.VoiceStateUpdated += Client_VoiceStateUpdated;
            ctx.Client.ChannelDeleted += Client_ChannelDeleted;

            async Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
            {
                var member = e.User as DiscordMember;

                if (member != null)
                {
                    bool joinedAnotherChannel = (e.Before == null || e.Before.Channel == null || e.Before.Channel == voiceChannel)
                                                && (e.After != null && e.After.Channel != null && e.After.Channel != voiceChannel);
                    bool disconnected = (e.Before != null && e.Before.Channel != null && e.Before.Channel == voiceChannel)
                                        && (e.After == null || e.After.Channel == null);
                    bool joinedThisChannel = (e.Before == null || e.Before.Channel == null || e.Before.Channel != voiceChannel)
                                            && (e.After != null && e.After.Channel != null && e.After.Channel == voiceChannel);
                    bool leftThisChannel = (e.Before != null && e.Before.Channel != null && e.Before.Channel == voiceChannel)
                                            && (e.After == null || e.After.Channel == null || e.After.Channel != voiceChannel);
                    var voiceMutedRole = ctx.Guild.Roles.Values.FirstOrDefault(role => role.Name.ToLower() == "voicemuted");

                    if (voiceMutedRole == null)
                    {
                        ErrorMessages.SendRoleNotExistMessage(client, e.Channel, "VoiceMuted");
                        return;
                    }

                    // Unmute if they join a channel without auto-mute
                    if (joinedAnotherChannel && !autoMuteChannels.Contains(e.After!.Channel!))
                    {
                        Debug.WriteLine("joined channel without auto-mute");
                        if (member.Roles.Contains(voiceMutedRole))
                        {
                            Debug.WriteLine("unmuting member");
                            await member.RevokeRoleAsync(voiceMutedRole);
                            await member.SetMuteAsync(false); // removing role while member is in VC will not automatically unmute them
                        }
                    }

                    // Unmute if they click Disconnect
                    if (disconnected)
                    {
                        Debug.WriteLine("disconnected");
                        if (member.Roles.Contains(voiceMutedRole))
                        {
                            Debug.WriteLine("unmuting member");
                            await member.RevokeRoleAsync(voiceMutedRole);
                        }
                    }

                    // Delete this channel if last person leaves
                    if (leftThisChannel)
                    {
                        Debug.WriteLine("left this channel");
                        if (voiceChannel.Users.Count == 0)
                        {
                            try
                            {
                                await voiceChannel.DeleteAsync();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Exception caught!\n{ex}");
                            }
                        }
                    }

                    // Mute or unmute when they join this auto-mute channel
                    if (joinedThisChannel)
                    {
                        Debug.WriteLine("joined this channel");
                        bool canMemberSpeak = membersUnMuted.Contains(member);

                        if (canMemberSpeak)
                        {
                            if (member.Roles.Contains(voiceMutedRole) || member.IsMuted)
                            {
                                Debug.WriteLine("unmuting member");
                                await member.RevokeRoleAsync(voiceMutedRole);
                                await member.SetMuteAsync(false); // removing role while member is in VC will not automatically unmute them
                            }
                        }
                        else
                        {
                            if (!member.Roles.Contains(voiceMutedRole) || !member.IsMuted)
                            {
                                Debug.WriteLine("muting member");
                                await member.GrantRoleAsync(voiceMutedRole);
                                await member.SetMuteAsync(true); // assigning role while member is in VC will not automatically mute them
                            }
                        }
                    }
                }
            }

            async Task Client_ChannelDeleted(DiscordClient client, ChannelDeleteEventArgs e)
            {
                if (e.Channel == voiceChannel)
                {
                    Debug.WriteLine($"Channel deleted: {e.Channel}");
                    autoMuteChannels.Remove(e.Channel);
                    client.VoiceStateUpdated -= Client_VoiceStateUpdated;
                    client.ChannelDeleted -= Client_ChannelDeleted;
                }
            }
        }



        /*        /// <summary>
                /// Creates a VC that checks whether member should be muted or
                /// unmuted when they join the VC, then server mutes or unmutes
                /// them. Cannot perform mute/unmute if member is not in a VC.
                /// </summary>
                [Command("automutevc"), Aliases("amvc")]
                public async Task AutoMuteCommand(CommandContext ctx, params DiscordMember[] membersUnMuted)
                {
                    DiscordChannel voiceChannel = await ctx.Guild.CreateVoiceChannelAsync($"Team {ctx.User.Username} (auto-mute)", ctx.Channel.Parent);
                    autoMuteChannels.Add(voiceChannel);

                    ctx.Client.VoiceStateUpdated += Client_VoiceStateUpdated;
                    ctx.Client.ChannelDeleted += Client_ChannelDeleted;

                    async Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
                    {
                        var member = e.User as DiscordMember;

                        if (member != null)
                        {
                            bool joinedAnotherChannel = (e.Before == null || e.Before.Channel == null || e.Before.Channel == voiceChannel)
                                                        && (e.After != null && e.After.Channel != null && e.After.Channel != voiceChannel);
                            bool disconnected = (e.Before != null && e.Before.Channel != null && e.Before.Channel == voiceChannel)
                                                && (e.After == null || e.After.Channel == null);
                            bool joinedThisChannel = (e.Before == null || e.Before.Channel == null || e.Before.Channel != voiceChannel)
                                                    && (e.After != null && e.After.Channel != null && e.After.Channel == voiceChannel);

                            // Unmute if they join a channel without auto-mute
                            if (member.IsMuted && joinedAnotherChannel && !autoMuteChannels.Contains(e.After!.Channel!))
                            {
                                Debug.WriteLine("unmuting member");
                                await member.SetMuteAsync(false);
                            }

                            // Delete this channel if last person leaves
                            if (joinedAnotherChannel || disconnected)
                            {
                                if (voiceChannel.Users.Count == 0)
                                {
                                    try
                                    {
                                        await voiceChannel.DeleteAsync();
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine($"Exception caught!\n{ex}");
                                    }
                                }
                            }

                            // Mute or unmute when they join this auto-mute channel
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
                    }

                    async Task Client_ChannelDeleted(DiscordClient client, ChannelDeleteEventArgs e)
                    {
                        if (e.Channel == voiceChannel)
                        {
                            Debug.WriteLine($"Channel deleted: {e.Channel}");
                            autoMuteChannels.Remove(e.Channel);
                            client.VoiceStateUpdated -= Client_VoiceStateUpdated;
                            client.ChannelDeleted -= Client_ChannelDeleted;
                        }
                    }
                }*/

        /*        [Command("deletecategorychannels")]
                public async Task DeleteCategoryChannelsCommand(CommandContext ctx)
                {
                    foreach (var c in ctx.Channel.Parent.Children)
                    {
                        if (c != ctx.Channel)
                        {
                            await c.DeleteAsync();
                        }
                    }
                }*/
    }
}
