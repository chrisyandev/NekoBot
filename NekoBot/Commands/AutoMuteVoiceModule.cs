using DSharpPlus;
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
        private static bool isAutoMuteSetup = false;

        /// <summary>
        /// Creates voice channel that mutes everyone except members that are pinged.
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
                if (!isAutoMuteSetup)
                {
                    ctx.Client.VoiceStateUpdated += JoinedAnyChannel;
                    isAutoMuteSetup = true;
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
                            bool canMemberSpeak = membersUnmuted.Contains(member);

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
                                var adminRole = ctx.Guild.Roles.Values.FirstOrDefault(role => role.Name.ToLower() == "admin");
                                bool isMemberAdmin = adminRole != null && member.Roles.Contains(adminRole);

                                if (!member.IsMuted && !isMemberAdmin)
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
        /// Creates voice channel that mutes everyone except first # of members who join.
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
                if (!isAutoMuteSetup)
                {
                    ctx.Client.VoiceStateUpdated += JoinedAnyChannel;
                    isAutoMuteSetup = true;
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
                            bool canMemberSpeak = membersUnmuted.Contains(member);
                            bool isBelowUnmutedLimit = voiceChannel.Users.Count <= numUnmuted;

                            // Admins should never be muted, and they should not count in # of unmuted members
                            if (canMemberSpeak || isBelowUnmutedLimit)
                            {
                                if (member.IsMuted)
                                {
                                    Debug.WriteLine("unmuting member");
                                    await member.SetMuteAsync(false);
                                }
                                if (!canMemberSpeak)
                                {
                                    membersUnmuted.Add(member);
                                    Debug.WriteLine($"members unmuted: {membersUnmuted.Count}");
                                }
                            }
                            else
                            {
                                var adminRole = ctx.Guild.Roles.Values.FirstOrDefault(role => role.Name.ToLower() == "admin");
                                bool isMemberAdmin = adminRole != null && member.Roles.Contains(adminRole);

                                if (!member.IsMuted && !isMemberAdmin)
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


        // Assigning VoiceMutedRole is redundant since it doesn't actual server mute/unmute members

        /*[Command("automutevc"), Aliases("amvc")]
        public async Task AutoMuteCommand(CommandContext ctx, params DiscordMember[] membersUnMuted)
        {
            // Check if role exists
            var voiceMutedRole = ctx.Guild.Roles.Values.FirstOrDefault(role => role.Name.ToLower() == "voicemuted");
            if (voiceMutedRole == null)
            {
                ErrorMessages.SendRoleNotExistMessage(ctx.Client, ctx.Channel, "VoiceMuted");
                return;
            }

            // Create new voice channel
            DiscordChannel voiceChannel = await ctx.Guild.CreateVoiceChannelAsync($"Team {ctx.Member?.DisplayName} (auto-mute)", ctx.Channel.Parent);
            autoMuteChannels.Add(voiceChannel);

            // Attach event handler once and only once
            if (!isAutoMuteSetup)
            {
                ctx.Client.VoiceStateUpdated += JoinedAnyChannel;
                isAutoMuteSetup = true;
            }

            // Attach event handlers
            ctx.Client.VoiceStateUpdated += Client_VoiceStateUpdated;
            ctx.Client.ChannelDeleted += Client_ChannelDeleted;

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
                    Debug.WriteLine($"Exception caught!\n{ex}");
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
                                Debug.WriteLine("removing VoiceMuted role");
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
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception caught!\n{ex}");
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
        }*/

    }
}
