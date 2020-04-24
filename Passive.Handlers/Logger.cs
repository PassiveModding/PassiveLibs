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
                SessionLogDirectory = Path.Combine(logDirectory, DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss"));
                SessionLogFileName = "log0.log";
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }
                if (!Directory.Exists(SessionLogDirectory))
                {
                    Directory.CreateDirectory(SessionLogDirectory);
                }

                filePath = Path.Combine(SessionLogDirectory, SessionLogFileName);
                writer = File.AppendText(filePath);
                writer.AutoFlush = true;
                writerLocker = new object();
            }
        }

        public LogLevel MinLogLevel { get; set; } = LogLevel.Verbose;

        public string LogDirectory { get; } = null;

        public string SessionLogDirectory { get; private set; }

        public string SessionLogFileName { get; private set; }

        public string filePath { get; private set; }

        private StreamWriter writer { get; set; }

        private int appendCount = 0;

        private int fileIteration = 0;

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
                    // Split into new file every 100,000 writes to reduce overall file sizes.
                    if (appendCount > 100000)
                    {
                        appendCount = 0;
                        fileIteration++;
                        SessionLogFileName = $"log{fileIteration}.log";
                        filePath = Path.Combine(SessionLogDirectory, SessionLogFileName);
                        writer = File.AppendText(filePath);
                        writer.AutoFlush = true;
                    }
                    writer.WriteLine(logContent);
                    appendCount++;
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