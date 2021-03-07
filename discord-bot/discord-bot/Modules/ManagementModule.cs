using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace discord_bot.Modules
{
    [Name("Bot Management"), RequireOwner, Group("manage")]
    public class ManagementModule : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;

        public ManagementModule(DiscordSocketClient client)
        {
            _client = client;
        }
        [Command("servers"), Summary("Lists the servers the bot is in")]
        public async Task ListServers()
        {
            var eb = new EmbedBuilder();
            eb.Title = "Servers";
            foreach (var guild in _client.Guilds)
            {
                eb.Description += $"**{guild.Name}** — `{guild.Id}`\n";
            }

            await ReplyAsync(embed: eb.Build());
        }
        [Command("leave"), Summary("Leave a server")]
        public async Task LeaveServer(ulong id)
        {
            if (_client.GetGuild(id) != null)
            {
                var g = _client.GetGuild(id);
                await ReplyAsync($"Leaving Guild: {g.Name}");
                await g.LeaveAsync();
            }
        }
    }
}