using DSharpPlus.Entities;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoBot.Utilities
{
    public static class ErrorMessages
    {
        public static async void SendUnAuthMessage(DiscordClient client, DiscordChannel channel)
        {
            try
            {
                await client.SendMessageAsync(channel,
                    "Error: Unauthorized. In the roles list (Server Settings -> Roles) " +
                    "make sure the bot's role is above the roles it can assign.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught!\n{ex}");
            }
        }

        public static async void SendRoleNotExistMessage(DiscordClient client, DiscordChannel channel, string roleName)
        {
            try
            {
                await client.SendMessageAsync(channel,
                    $"Error: Role doesn't exist. Make sure {roleName} role is in the " +
                    $"roles list (Server Settings -> Roles)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught!\n{ex}");
            }
        }
    }
}
