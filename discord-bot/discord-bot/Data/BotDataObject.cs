using System.Collections.Concurrent;

namespace discord_bot.Data
{
    public class BotDataObject
    {
        public GlobalPersistentData GlobalPersistentData { get; set; } = new GlobalPersistentData();

        public ConcurrentDictionary<ulong, GuildPersistentData> GuildPersistentData { get; set; } =
            new ConcurrentDictionary<ulong, GuildPersistentData>();
        public ConcurrentDictionary<ulong, UserPersistentData> UserPersistentData { get; set; } =
            new ConcurrentDictionary<ulong, UserPersistentData>();
    }
}