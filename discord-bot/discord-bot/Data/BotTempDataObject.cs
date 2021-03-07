using System.Collections.Concurrent;

namespace discord_bot.Data
{
    public class BotTempDataObject
    {
        public GlobalData GlobalData { get; set; } = new GlobalData();

        public ConcurrentDictionary<ulong, GuildData> GuildData { get; set; } =
            new ConcurrentDictionary<ulong, GuildData>();
        public ConcurrentDictionary<ulong, UserData> UserData { get; set; } =
            new ConcurrentDictionary<ulong, UserData>();
    }
}