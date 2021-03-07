using System.Threading;
using Discord.Commands;
using Discord.WebSocket;
using discord_bot.Utils;
using Nito.AsyncEx;

namespace discord_bot.Services
{
    public class EventService
    {
        /// <summary>
        /// Bot shutdown signal
        /// </summary>
        public CancellationToken StopToken;
        /// <summary>
        /// Bot shutdown waiter
        /// </summary>
        public AsyncManualResetEvent StopWaiter = new AsyncManualResetEvent(true);
        /// <summary>
        /// Delegate for receiving user messages.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Return true if the message should be further processed</returns>
        public delegate bool UserMessageDelegate(SocketCommandContext context);
        /// <summary>
        /// Delegate for receiving messages.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Return true if the message should be further processed</returns>
        public delegate bool MessageDelegate(SocketMessage message);
        /// <summary>
        /// Called when a message sent by a user is received
        /// </summary>
        public event UserMessageDelegate OnUserMessage;
        /// <summary>
        /// Called when a message is received
        /// </summary>
        public event MessageDelegate OnMessage;
        /// <summary>
        /// Internal function to be called by CommandHandler
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool ProcessMessage(SocketMessage msg)
        {
            if (OnMessage != null)
            {
                return OnMessage.Invoke(msg);
            }
            return true;
        }
        /// <summary>
        /// Internal function to be called by CommandHandler
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool ProcessUserMessage(SocketCommandContext ctx)
        {
            if (OnUserMessage != null)
            {
                return OnUserMessage.Invoke(ctx);
            }
            return true;
        }
        private int _waiting = 0;
        /// <summary>
        /// Create a new shutdown waiter
        /// </summary>
        /// <returns></returns>
        public Waiter GetWaiter()
        {
            _waiting++;
            StopWaiter.Reset();
            return new Waiter(() =>
            {
                _waiting--;
                if (_waiting == 0)
                {
                    StopWaiter.Set();
                }
            });
        }
    }
}