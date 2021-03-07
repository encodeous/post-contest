using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using discord_bot.Data;
using discord_bot.Utils;

namespace discord_bot.Services
{
    public class StorageService
    {
        private BotDataObject _data = null;
        private BotTempDataObject _tempData = null;

        public StorageService(EventService events)
        {
            _tempData = new BotTempDataObject();
            var waiter = events.GetWaiter();
            "Loading Data".Log();
            if (!Directory.Exists("data")) Directory.CreateDirectory("data");
            if (File.Exists("data/bot.data"))
            {
                var k = File.ReadAllText("data/bot.data");
                _data = JsonSerializer.Deserialize<BotDataObject>(k);
            }
            else
            {
                _data = new BotDataObject();
                File.WriteAllText("data/bot.data", JsonSerializer.Serialize(_data));
            }

            Task.Run(async () =>
            {
                using (waiter)
                {
                    try
                    {
                        while (!events.StopToken.IsCancellationRequested)
                        {
                            await Task.Delay(10000, events.StopToken);
                            if (!events.StopToken.IsCancellationRequested)
                                File.WriteAllText("data/bot.data", JsonSerializer.Serialize(_data));
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    await Task.Delay(1000);
                    "Saving Data".Log();
                    File.WriteAllText("data/bot.data", JsonSerializer.Serialize(_data));
                }
            });
        }
        public GuildPersistentData GetGuildPersistentData(ulong guildId)
        {
            if (!_data.GuildPersistentData.ContainsKey(guildId)) _data.GuildPersistentData[guildId] = new GuildPersistentData() {GuildId = guildId};
            return _data.GuildPersistentData[guildId];
        }
        
        public UserPersistentData GetUserPersistentData(ulong userId)
        {
            if (!_data.UserPersistentData.ContainsKey(userId)) _data.UserPersistentData[userId] = new UserPersistentData() {UserId = userId};
            return _data.UserPersistentData[userId];
        }
        public GlobalPersistentData GetBotPersistentData()
        {
            return _data.GlobalPersistentData;
        }
        public GuildData GetGuildData(ulong guildId)
        {
            if (!_tempData.GuildData.ContainsKey(guildId)) _tempData.GuildData[guildId] = new GuildData() {GuildId = guildId};
            return _tempData.GuildData[guildId];
        }
        
        public UserData GetUserData(ulong userId)
        {
            if (!_tempData.UserData.ContainsKey(userId)) _tempData.UserData[userId] = new UserData() {UserId = userId};
            return _tempData.UserData[userId];
        }
        public GlobalData GetBotData()
        {
            return _tempData.GlobalData;
        }
    }
}