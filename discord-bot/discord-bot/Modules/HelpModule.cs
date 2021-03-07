using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using discord_bot.Services;
using discord_bot.Utils;

namespace discord_bot.Modules
{
    [Name("Help Command")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly HelpGenerator _generator;
        public StorageService Storage { get; set; }
        public HelpModule(HelpGenerator generator)
        {
            _generator = generator;
        }
        [Command("help"), Summary("Show help command")]
        public async Task Help([Remainder] string command_or_module = "")
        {
            await ReplyAsync("", false, await _generator.GetHelp(command_or_module));
        }
        [Command("prefix"), Summary("Get current bot prefix")]
        public async Task Prefix()
        {
            await ReplyAsync($"The bot prefix is `{Storage.GetGuildPersistentData(Context.Guild.Id).Prefix}`");
        }
    }
}