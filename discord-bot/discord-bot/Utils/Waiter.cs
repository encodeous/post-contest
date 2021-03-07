using System;
using discord_bot.Services;

namespace discord_bot.Utils
{
    public class Waiter : IDisposable
    {
        private Action _stop;
        private bool Waiting = true;
        public Waiter(Action stop)
        {
            _stop = stop;
        }
        public void Dispose()
        {
            lock (this)
            {
                if (Waiting)
                {
                    Waiting = false;
                    _stop.Invoke();
                }
            }
        }
    }
}