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

        [Command("roles")]
        public async Task PollCommand(CommandContext ctx)
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
