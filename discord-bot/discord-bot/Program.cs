using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using discord_bot.Services;
using discord_bot.Utils;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx.Synchronous;

namespace discord_bot
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private CommandHandler _handler;
        private EventService _events;
        private CancellationTokenSource _stopSource;
        private static bool _exitSignal = false;

        public static void Main(string[] args)
        {
            if (Config.NeverShutDown)
            {
                while (!_exitSignal)
                {
                    try
                    {
                        new Program().MainAsync().WaitWithoutException();
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"{e.Message} {e.StackTrace}");
                        "Bot Crashed! Restarting in 20s!".Log();
                        Thread.Sleep(20000);
                    }
                }
            }
            else
            {
                new Program().MainAsync().WaitWithoutException();
            }
            Console.WriteLine("Bot has shut down! Bye!");
        }

        public async Task MainAsync()
        {
            Console.CancelKeyPress += ExitHandler;
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _stopSource = new CancellationTokenSource();
            _events = new EventService();
            _events.StopToken = _stopSource.Token;
            _client.Log += Log;
            // Load Token
            string token;
            if (File.Exists("token.txt"))
            {
                token = File.ReadAllText("token.txt");
            }
            else
            {
                File.WriteAllText("token.txt","Paste your bot token here (replace all text)!\nGet it from https://discord.com/developers/applications");
                Console.WriteLine("Token not found! please paste it into token.txt!");
                return;
            }

            try
            { 
                // Prepare Discord Client
                await _client.LoginAsync(TokenType.Bot, token);
                _stopSource.Token.Register(async () => await _client.StopAsync());
                // Start Bot Services
                var waiter = _events.GetWaiter();
                _stopSource.Token.Register(async () => waiter.Dispose());
            
                var service = BuildServiceProvider();
                WarmUp(service);
                _handler = new CommandHandler(_commands, _client, service);
                // Start the bot
                await _client.StartAsync();
                await _handler.SetupAsync();
                await _events.StopWaiter.WaitAsync();
            }
            finally
            {
                if (!_stopSource.IsCancellationRequested)
                {
                    _stopSource.Cancel();
                    await _events.StopWaiter.WaitAsync();
                }
            }
        }

        private void ExitHandler(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _stopSource.Cancel();
            _exitSignal = true;
        }

        private Task Log(LogMessage msg)
        {
            msg.ToString().Log();
            return Task.CompletedTask;
        }
        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton(_events)
            .AddSingleton<StorageService>()
            .AddScoped<HelpGenerator>()
            .AddScoped<RequestScope>()
            .BuildServiceProvider();
        /// <summary>
        /// Warm up services
        /// </summary>
        /// <param name="services"></param>
        public void WarmUp(IServiceProvider services)
        {
            services.GetService<StorageService>();
        }
    }
}