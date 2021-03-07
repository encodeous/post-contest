using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using discord_bot.Services;

namespace discord_bot.Modules
{
    [Name("Settings"), Group("settings")]
    public class SettingsModule : ModuleBase<SocketCommandContext>
    {
        [Name("Server"), Summary("Server Settings"), Group("server")]
        public class ServerModule : ModuleBase<SocketCommandContext>
        {
            private StorageService _service;

            public ServerModule(StorageService svc)
            {
                _service = svc;
            }
            [Command("prefix"), Summary("Set bot prefix"), RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task SetPrefix(string prefix)
            {
                _service.GetGuildPersistentData(Context.Guild.Id).Prefix = prefix;
                await ReplyAsync($"Prefix successfully set to `{prefix}`");
            }
        }
    }
}