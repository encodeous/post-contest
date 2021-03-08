using System.Collections.Generic;
using discord_bot.Utils;

namespace discord_bot.Data
{
    public class GuildPersistentData
    {
        public ulong GuildId { get; set; }
        public string Prefix { get; set; } = Config.DefaultPrefix;
        public bool HasSetup { get; set; } = false;
        public ulong ConfidentialCategory { get; set; } = 0;
        public ulong PublicCategory { get; set; } = 0;
        public Dictionary<string, ulong> ActiveChannels { get; set; } = new Dictionary<string, ulong>();
    }
}