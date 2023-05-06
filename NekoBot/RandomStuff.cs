using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NekoBot
{
    public static class RandomStuff
    {
        public static void Initialize(DiscordClient client)
        {
            Console.WriteLine($"Initializing RandomStuff ...");

            ulong targetGuildId = 1079829402482921482;

            if (client != null)
            {
                Console.WriteLine($"Finding Guild ...");
                foreach (DiscordGuild guild in client.Guilds.Values)
                {
                    Console.WriteLine($"Found {guild.Id} ...");
                    if (guild.Id == targetGuildId)
                    {
                        Console.WriteLine($"RandomStuff initialized");
                        client.MessageCreated += Client_MessageCreated;
                        break;
                    }
                }
            }

            async Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
            {
                if (e.Guild.Id == targetGuildId && Regex.IsMatch(e.Message.Content, @"\b(HR)\b", RegexOptions.IgnoreCase))
                {
                    Console.WriteLine($"Reacting with HR emoji in {e.Guild.Name}");
                    try
                    {
                        DiscordEmoji emoji = DiscordEmoji.FromName(client, ":HRDepartment:");
                        await e.Message.CreateReactionAsync(emoji);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception caught when reacting with HR emoji\n{ex}");
                    }
                }
            }
        }
    }
}
