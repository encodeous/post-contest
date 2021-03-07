using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace discord_bot.Utils
{
    static class AsyncLogger
    {
        private static object LockObject = new object();

        public static void Log(this string s)
        {
            lock (LockObject)
            {
                Console.WriteLine(s);
            }
        }
    }
}
