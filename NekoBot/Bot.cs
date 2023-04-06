using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace NekoBot
{
    public class Bot
    {
        public void Init(DiscordClient client)
        {
            client.MessageCreated += Client_MessageCreated;
        }

        private async Task Client_MessageCreated(DiscordClient client, MessageCreateEventArgs args)
        {
            if (args.Message.Content.ToLower().StartsWith("ping"))
            {
                await args.Message.RespondAsync("pong!");
            }
                
        }
    }
}
