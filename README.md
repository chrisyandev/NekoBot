# NekoBot

A Discord bot for personal use.

## Technologies Used

- [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)
- [DSharpPlus 4.4.0](https://dsharpplus.github.io/DSharpPlus/)

## Features

- `!automutevc` command: Creates a voice channel where only allowed members can speak. Other members are server muted. Handles unmuting members when they join a voice channel in which they're allowed to speak. Automatically deletes voice channel when last person leaves. 
- `!roles` command: Creates an embed that assigns/unassigns Tank, Healer, and DPS role when user clicks on the role's respective reaction.
- `!createinvite` command: Creates an invite to the first channel member should see upon joining. Useful for setting a custom expiry time and # of uses.

## Notes

- Under Server Settings -> Roles, make sure the bot's role comes before the roles that it can assign, otherwise throws an UnauthorizedException.