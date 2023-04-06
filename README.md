# NekoBot

A Discord bot for personal use. Right now only assigns hard-coded roles for different emoji reactions.

## Technologies Used

- [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)
- [DSharpPlus](https://dsharpplus.github.io/DSharpPlus/)

## Notes

Under Server Settings -> Roles, make sure the bot's role comes before the roles that it can assign, otherwise throws an UnauthorizedException.