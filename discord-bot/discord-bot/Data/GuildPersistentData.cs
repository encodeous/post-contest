using discord_bot.Utils;

namespace discord_bot.Data
{
    public class GuildPersistentData
    {
        public ulong GuildId { get; set; }
        public string Prefix { get; set; } = Config.DefaultPrefix;
    }
}