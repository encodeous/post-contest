using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using discord_bot.Services;
using discord_bot.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace discord_bot
{
    public class CommandHandler
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;

        public CommandHandler(CommandService commands, DiscordSocketClient client, IServiceProvider services)
        {
            _commands = commands;
            _client = client;
            _services = services;
        }

        public async Task SetupAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var events = _services.GetService<EventService>();
            if (!events.ProcessMessage(messageParam)) return;
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            
            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);
            if (!events.ProcessUserMessage(context)) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            string prefix = Config.DefaultPrefix;
            if (message.Channel is IGuildChannel c)
            {
                var storage = _services.GetService<StorageService>();
                prefix = storage.GetGuildPersistentData(c.GuildId).Prefix;
            }
            
            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasStringPrefix(prefix, ref argPos) ||
                  message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            var k = _services.CreateScope();
            k.ServiceProvider.GetService<RequestScope>().Context = context;
            var result = await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: k.ServiceProvider);
            if (!result.IsSuccess && Config.ShowErrorMessages)
            {
                await context.Channel.SendMessageAsync(result.Error.ToString() + " " + result.ErrorReason);
            }
        }
    }
}