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
        private static List<DiscordChannel> autoMuteChannels = new();
        private static List<ulong> guildsWithAutoMuteSetup = new();

        /// <summary>
        /// Creates a voice channel that mutes everyone except members that are pinged and the member that invoked this command.
        /// </summary>
        [Command("automutevc"), Aliases("amvc")]
        public async Task AutoMuteCommand(CommandContext ctx, params DiscordMember[] membersUnmuted)
        {
            DiscordChannel? voiceChannel = null;
            
            try
            {
                Console.WriteLine("creating voice channel");

                // Create new voice channel
                voiceChannel = await ctx.Guild.CreateVoiceChannelAsync($"Team {ctx.Member?.DisplayName} (auto-mute)", ctx.Channel.Parent);
                autoMuteChannels.Add(voiceChannel);

                // Attach event handler once and only once
                if (!guildsWithAutoMuteSetup.Contains(ctx.Guild.Id))
                {
                    ctx.Client.VoiceStateUpdated += JoinedAnyChannel;
                    guildsWithAutoMuteSetup.Add(ctx.Guild.Id);
                }

                // Attach event handlers
                ctx.Client.VoiceStateUpdated += Client_VoiceStateUpdated;
                ctx.Client.ChannelDeleted += Client_ChannelDeleted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"*Exception caught while trying to create and setup a new voice channel*\n{ex}");
            }

            // Unmutes members that join any VC that is not auto-mute since
            // currently there is no way to preview members disconnecting from
            // VC, and members cannot be unmuted if they are not in a VC.
            // Only 1 of this event handler should be subscribed.
            async Task JoinedAnyChannel(DiscordClient client, VoiceStateUpdateEventArgs e)
            {
                try
                {
                    var member = e.User as DiscordMember;

                    if (member != null)
                    {
                        bool joinedAChannel = (e.Before == null || e.Before.Channel == null) && (e.After != null && e.After.Channel != null);

                        // Unmute if they join a channel without auto-mute
                        if (joinedAChannel)
                        {
                            Debug.WriteLine("joined a channel");
                            if (autoMuteChannels.Contains(e.After!.Channel!))
                            {
                                Debug.WriteLine("channel is auto-mute");
                            }
                            else
                            {
                                Debug.WriteLine("channel is not auto-mute");
                                Debug.WriteLine("removing server mute");
                                await member.SetMuteAsync(false); // removing role while member is in VC will not automatically unmute them
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"*Exception caught while executing JoinedAnyChannel()*\n{ex}");
                }
            }

            async Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
            {
                try
                {
                    var member = e.User as DiscordMember;

                    if (member != null)
                    {
                        bool joinedAnotherChannel = (e.Before != null && e.Before.Channel != null && e.Before.Channel == voiceChannel)
                                                    && (e.After != null && e.After.Channel != null && e.After.Channel != voiceChannel);
                        bool disconnected = (e.Before != null && e.Before.Channel != null && e.Before.Channel == voiceChannel)
                                            && (e.After == null || e.After.Channel == null);
                        bool joinedThisChannel = (e.Before == null || e.Before.Channel == null || e.Before.Channel != voiceChannel)
                                                && (e.After != null && e.After.Channel != null && e.After.Channel == voiceChannel);
                        bool leftThisChannel = (e.Before != null && e.Before.Channel != null && e.Before.Channel == voiceChannel)
                                                && (e.After == null || e.After.Channel == null || e.After.Channel != voiceChannel);

                        // Unmute if they join another channel from this channel, and it has no auto-mute
                        if (joinedAnotherChannel && !autoMuteChannels.Contains(e.After!.Channel!))
                        {
                            Debug.WriteLine("joined channel without auto-mute");
                            Debug.WriteLine("unmuting member");
                            await member.SetMuteAsync(false);
                        }

                        if (disconnected)
                        {
                            Debug.WriteLine("disconnected");
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
                                catch (Exception)
                                {
                                    Debug.WriteLine("Couldn't delete VC");
                                    throw;
                                }
                            }
                        }

                        // Mute or unmute when they join this auto-mute channel
                        if (joinedThisChannel)
                        {
                            Debug.WriteLine("joined this channel");

                            var adminRole = ctx.Guild.Roles.Values.FirstOrDefault(role => role.Name.ToLower() == "admin");
                            bool isMemberAdmin = adminRole != null && member.Roles.Contains(adminRole);
                            bool canMemberSpeak = membersUnmuted.Contains(member) || isMemberAdmin || member == ctx.Member;

                            if (canMemberSpeak)
                            {
                                if (member.IsMuted)
                                {
                                    Debug.WriteLine("unmuting member");
                                    await member.SetMuteAsync(false);
                                }
                            }
                            else
                            {
                                if (!member.IsMuted)
                                {
                                    Debug.WriteLine("muting member");
                                    await member.SetMuteAsync(true);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"*Exception caught while executing Client_VoiceStateUpdated()*\n{ex}");
                }
            }

            Task Client_ChannelDeleted(DiscordClient client, ChannelDeleteEventArgs e)
            {
                if (e.Channel == voiceChannel)
                {
                    Debug.WriteLine($"Channel deleted: {e.Channel}");
                    autoMuteChannels.Remove(e.Channel);
                    client.VoiceStateUpdated -= Client_VoiceStateUpdated;
                    client.ChannelDeleted -= Client_ChannelDeleted;
                }

                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Creates a voice channel that mutes everyone except the first members who join up to the unmuted limit.
        /// </summary>
        [Command("automutevc")]
        public async Task AutoMuteCommand(CommandContext ctx, int numUnmuted)
        {
            if (numUnmuted < 0)
            {
                return;
            }

            DiscordChannel? voiceChannel = null;
            List<DiscordMember> membersUnmuted = new(); // First members to join while under unmuted limit

            try
            {
                Console.WriteLine("creating voice channel");

                // Create new voice channel
                voiceChannel = await ctx.Guild.CreateVoiceChannelAsync($"Team {ctx.Member?.DisplayName} (auto-mute)", ctx.Channel.Parent);
                autoMuteChannels.Add(voiceChannel);

                // Attach event handler once and only once
                if (!guildsWithAutoMuteSetup.Contains(ctx.Guild.Id))
                {
                    ctx.Client.VoiceStateUpdated += JoinedAnyChannel;
                    guildsWithAutoMuteSetup.Add(ctx.Guild.Id);
                }

                // Attach event handlers
                ctx.Client.VoiceStateUpdated += Client_VoiceStateUpdated;
                ctx.Client.ChannelDeleted += Client_ChannelDeleted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"*Exception caught while trying to create and setup a new voice channel*\n{ex}");
            }

            // Unmutes members that join any VC that is not auto-mute since
            // currently there is no way to preview members disconnecting from
            // VC, and members cannot be unmuted if they are not in a VC.
            // Only 1 of this event handler should be subscribed.
            async Task JoinedAnyChannel(DiscordClient client, VoiceStateUpdateEventArgs e)
            {
                try
                {
                    var member = e.User as DiscordMember;

                    if (member != null)
                    {
                        bool joinedAChannel = (e.Before == null || e.Before.Channel == null) && (e.After != null && e.After.Channel != null);

                        // Unmute if they join a channel without auto-mute
                        if (joinedAChannel)
                        {
                            Debug.WriteLine("joined a channel");
                            if (autoMuteChannels.Contains(e.After!.Channel!))
                            {
                                Debug.WriteLine("channel is auto-mute");
                            }
                            else
                            {
                                Debug.WriteLine("channel is not auto-mute");
                                Debug.WriteLine("removing server mute");
                                await member.SetMuteAsync(false); // removing role while member is in VC will not automatically unmute them
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"*Exception caught while executing JoinedAnyChannel()*\n{ex}");
                }
            }

            async Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
            {
                try
                {
                    var member = e.User as DiscordMember;

                    if (member != null)
                    {
                        bool joinedAnotherChannel = (e.Before != null && e.Before.Channel != null && e.Before.Channel == voiceChannel)
                                                    && (e.After != null && e.After.Channel != null && e.After.Channel != voiceChannel);
                        bool disconnected = (e.Before != null && e.Before.Channel != null && e.Before.Channel == voiceChannel)
                                            && (e.After == null || e.After.Channel == null);
                        bool joinedThisChannel = (e.Before == null || e.Before.Channel == null || e.Before.Channel != voiceChannel)
                                                && (e.After != null && e.After.Channel != null && e.After.Channel == voiceChannel);
                        bool leftThisChannel = (e.Before != null && e.Before.Channel != null && e.Before.Channel == voiceChannel)
                                                && (e.After == null || e.After.Channel == null || e.After.Channel != voiceChannel);

                        // Unmute if they join another channel from this channel, and it has no auto-mute
                        if (joinedAnotherChannel && !autoMuteChannels.Contains(e.After!.Channel!))
                        {
                            Debug.WriteLine("joined channel without auto-mute");
                            Debug.WriteLine("unmuting member");
                            await member.SetMuteAsync(false);
                        }

                        if (disconnected)
                        {
                            Debug.WriteLine("disconnected");
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
                                catch (Exception)
                                {
                                    Debug.WriteLine("Couldn't delete VC");
                                    throw;
                                }
                            }
                        }

                        // Mute or unmute when they join this auto-mute channel
                        if (joinedThisChannel)
                        {
                            Debug.WriteLine("joined this channel");

                            var adminRole = ctx.Guild.Roles.Values.FirstOrDefault(role => role.Name.ToLower() == "admin");
                            bool isMemberAdmin = adminRole != null && member.Roles.Contains(adminRole);

                            // If member has not been added to list, add them if unmuted limit has not been reached
                            // Admins should never be muted, but they should count as part of the group
                            if (membersUnmuted.Count < numUnmuted && !membersUnmuted.Contains(member))
                            {
                                membersUnmuted.Add(member);
                            }

                            bool canMemberSpeak = membersUnmuted.Contains(member) || isMemberAdmin;
                            
                            if (canMemberSpeak)
                            {
                                if (member.IsMuted)
                                {
                                    Debug.WriteLine("unmuting member");
                                    await member.SetMuteAsync(false);
                                }
                            }
                            else
                            {
                                if (!member.IsMuted)
                                {
                                    Debug.WriteLine("muting member");
                                    await member.SetMuteAsync(true);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"*Exception caught while executing Client_VoiceStateUpdated()*\n{ex}");
                }
            }

            Task Client_ChannelDeleted(DiscordClient client, ChannelDeleteEventArgs e)
            {
                if (e.Channel == voiceChannel)
                {
                    Debug.WriteLine($"Channel deleted: {e.Channel}");
                    autoMuteChannels.Remove(e.Channel);
                    client.VoiceStateUpdated -= Client_VoiceStateUpdated;
                    client.ChannelDeleted -= Client_ChannelDeleted;
                }

                return Task.CompletedTask;
            }
        }

    }
}
