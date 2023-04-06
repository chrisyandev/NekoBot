using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoBot.Commands
{
    public class ReactionRoleModule : BaseCommandModule
    {
        private const string DpsRoleName = "dps";
        private const string HealerRoleName = "healer";
        private const string TankRoleName = "tank";

        /// <summary>
        /// Anyone can react to this message to obtain a role. Removing their reaction removes their role.
        /// </summary>
        [Command("roles")]
        public async Task RolesCommand(CommandContext ctx)
        {
            Dictionary<string, DiscordEmoji> emojiOptions = new()
            {
                [DpsRoleName] = DiscordEmoji.FromName(ctx.Client, ":crossed_swords:"),
                [HealerRoleName] = DiscordEmoji.FromName(ctx.Client, ":green_square:"),
                [TankRoleName] = DiscordEmoji.FromName(ctx.Client, ":shield:")
            };

            var msgBuilder = new DiscordMessageBuilder();
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle("Role Selection")
                .WithDescription("Select your role to unlock channels")
                .WithThumbnail(ctx.Client.CurrentUser.AvatarUrl);
            msgBuilder.AddEmbed(embed);

            var msg = await ctx.Channel.SendMessageAsync(msgBuilder);

            foreach (var emoji in emojiOptions.Values)
            {
                await msg.CreateReactionAsync(emoji);
            }

            ctx.Client.MessageReactionAdded += async (client, e) =>
            {
                if (e.Message != msg)
                {
                    return;
                }

                var member = e.User as DiscordMember;
                DiscordRole? role = null;
                var allRoles = ctx.Guild.Roles.Values;

                if (e.Emoji == emojiOptions[DpsRoleName])
                {
                    role = allRoles.FirstOrDefault(role => role.Name.ToLower() == DpsRoleName);
                }
                else if (e.Emoji == emojiOptions[TankRoleName])
                {
                    role = allRoles.FirstOrDefault(role => role.Name.ToLower() == TankRoleName);
                }
                else if (e.Emoji == emojiOptions[HealerRoleName])
                {
                    role = allRoles.FirstOrDefault(role => role.Name.ToLower() == HealerRoleName);
                }
                else
                {
                    await e.Message.DeleteReactionsEmojiAsync(e.Emoji);
                }

                if (member != null && role != null)
                {
                    await member.GrantRoleAsync(role);
                }
            };

            ctx.Client.MessageReactionRemoved += async (client, e) =>
            {
                if (e.Message != msg)
                {
                    return;
                }

                var member = e.User as DiscordMember;
                DiscordRole? role = null;
                var allRoles = ctx.Guild.Roles.Values;

                if (e.Emoji == emojiOptions[DpsRoleName])
                {
                    role = allRoles.FirstOrDefault(role => role.Name.ToLower() == DpsRoleName);
                }
                else if (e.Emoji == emojiOptions[TankRoleName])
                {
                    role = allRoles.FirstOrDefault(role => role.Name.ToLower() == TankRoleName);
                }
                else if (e.Emoji == emojiOptions[HealerRoleName])
                {
                    role = allRoles.FirstOrDefault(role => role.Name.ToLower() == HealerRoleName);
                }
                else
                {
                    await e.Message.DeleteReactionsEmojiAsync(e.Emoji);
                }

                if (member != null && role != null)
                {
                    await member.RevokeRoleAsync(role);
                }
            };
        }

        /// <summary>
        /// WIP. Only adds a single role to user who created the command.
        /// </summary>
        //[Command("singlerole")]
        public async Task SingleRoleCommand(CommandContext ctx)
        {
            Dictionary<string, DiscordEmoji> emojiOptions = new()
            {
                [DpsRoleName] = DiscordEmoji.FromName(ctx.Client, ":crossed_swords:"),
                [HealerRoleName] = DiscordEmoji.FromName(ctx.Client, ":green_square:"),
                [TankRoleName] = DiscordEmoji.FromName(ctx.Client, ":shield:")
            };

            var msgBuilder = new DiscordMessageBuilder();
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle("Role Selection")
                .WithDescription("Select your role to unlock channels")
                .WithThumbnail(ctx.Client.CurrentUser.AvatarUrl);
            msgBuilder.AddEmbed(embed);

            var msg = await ctx.Channel.SendMessageAsync(msgBuilder);

            foreach(var emoji in emojiOptions.Values)
            {
                await msg.CreateReactionAsync(emoji);
            }

            var interactivity = ctx.Client.GetInteractivity();

            var result = await interactivity.WaitForReactionAsync(e =>
                e.Message == msg
                && e.User == ctx.User
                && emojiOptions.Values.Contains(e.Emoji));

            var member = ctx.Member;
            DiscordRole? role = null;
            var allRoles = ctx.Guild.Roles.Values;

            if (result.Result.Emoji == emojiOptions[DpsRoleName])
            {
                role = allRoles.FirstOrDefault(role => role.Name.ToLower() == DpsRoleName);
            }
            else if (result.Result.Emoji == emojiOptions[TankRoleName])
            {
                role = allRoles.FirstOrDefault(role => role.Name.ToLower() == TankRoleName);
            }
            else if (result.Result.Emoji == emojiOptions[HealerRoleName])
            {
                role = allRoles.FirstOrDefault(role => role.Name.ToLower() == HealerRoleName);
            }

            if (member != null && role != null)
            {
                await member.GrantRoleAsync(role);
            }   
        }
    }
}
