using System;

namespace Passive
{
    public class Logger
    {
        public enum LogLevel
        {
            Info,
            Error,
            Verbose,
            Debug,
            Warn
        }

        public enum Source
        {
            Bot,
            Cmd
        }

        private string TimeStamp => DateTime.UtcNow.ToString("HH:mm:ss dd/MM/yy");

        public void Log(string message, string source, LogLevel level = LogLevel.Info)
        {
            Console.WriteLine($"[{TimeStamp}]" +
                $"[{level.ToString().ToUpper().PadRight(4).Substring(0, 4)}]" +
                $"[{source.ToUpper()}]{message}");
        }

        public void Log(string message, Source source, LogLevel level = LogLevel.Info)
        {
            Log(message, source.ToString(), level);
        }
    }
}