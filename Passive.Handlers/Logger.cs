using System;
using System.IO;

namespace Passive.Logging
{
    public partial class Logger : IDisposable
    {
        public Logger(LogLevel minLogLevel, string logDirectory = null)
        {
            MinLogLevel = minLogLevel;
            if (logDirectory != null)
            {
                LogDirectory = logDirectory;
                SessionLogFileName = "log-" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".log";
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                var filePath = Path.Combine(LogDirectory, SessionLogFileName);
                writer = File.AppendText(filePath);
                writerLocker = new object();
            }
        }

        public LogLevel MinLogLevel { get; set; } = LogLevel.Verbose;

        public string LogDirectory { get; } = null;

        public string SessionLogFileName { get; }

        private StreamWriter writer { get; }

        public object writerLocker;

        private string TimeStamp => DateTime.UtcNow.ToString("HH:mm:ss dd/MM/yy");

        public void Log(string message, string source, LogLevel level = LogLevel.Info)
        {
            if (level < MinLogLevel) return;

            var logContent = $"[{TimeStamp}]" +
                $"[{level.ToString().ToUpper().PadRight(4).Substring(0, 4)}]" +
                $"[{source.ToUpper()}]{message}";
            Console.WriteLine(logContent);

            if (LogDirectory != null)
            {
                lock (writerLocker)
                {
                    writer.WriteLine(logContent);
                }
            }
        }

        public void Log(string message, Source source, LogLevel level = LogLevel.Info)
        {
            Log(message, source.ToString(), level);
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}