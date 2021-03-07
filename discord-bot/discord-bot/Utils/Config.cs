namespace discord_bot.Utils
{
    public static class Config
    {
        /// <summary>
        /// Prevents the bot from ever shutting down! Except for Ctrl-C signal
        /// </summary>
        public static bool NeverShutDown = true;

        public static string DefaultPrefix = "!";
        public static string BotName = "Default-Bot";
        // Help Generator Configuration
        /// <summary>
        /// Should a help page/command be hidden if the user can't use it?
        /// </summary>
        public static bool PermissionBasedGeneration = true;
        /// <summary>
        /// Should error messages be shown when commands are run?
        /// </summary>
        public static bool ShowErrorMessages = true;
    }
}