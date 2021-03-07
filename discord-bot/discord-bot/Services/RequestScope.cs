using Discord;
using Discord.Commands;
using discord_bot.Data;

namespace discord_bot.Services
{
    public class RequestScope
    {
        private StorageService _storage;

        public RequestScope(StorageService svc)
        {
            _storage = svc;
        }
        public SocketCommandContext Context;
        public GuildPersistentData GetGuildPersistentData()
        {
            return _storage.GetGuildPersistentData(Context.Guild.Id);
        }
        
        public UserPersistentData GetUserPersistentData()
        {
            return _storage.GetUserPersistentData(Context.User.Id);
        }
        public GlobalPersistentData GetBotPersistentData()
        {
            return _storage.GetBotPersistentData();
        }
        public GuildData GetGuildData()
        {
            return _storage.GetGuildData(Context.Guild.Id);
        }
        
        public UserData GetUserData()
        {
            return _storage.GetUserData(Context.User.Id);
        }
        public GlobalData GetBotData()
        {
            return _storage.GetBotData();
        }
    }
}