using System;

namespace Passive
{
    public class Logger
    {
        public Logger(LogLevel minLogLevel)
        {
            MinLogLevel = minLogLevel;
        }

        public enum LogLevel
        {
            Info = 2,

            Error = 4,

            Verbose = 0,

            Debug = 1,

            Warn = 3
        }

        public enum Source
        {
            Bot,

            Cmd
        }

        public LogLevel MinLogLevel { get; set; } = LogLevel.Verbose;

        private string TimeStamp => DateTime.UtcNow.ToString("HH:mm:ss dd/MM/yy");

        public void Log(string message, string source, LogLevel level = LogLevel.Info)
        {
            if (level < MinLogLevel) return;

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