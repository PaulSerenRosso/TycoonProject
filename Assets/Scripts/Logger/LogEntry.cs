using System;

namespace Logger
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public class LogEntry
    {
        public LogLevel Level { get; }
        public string Message { get; }
        public string Source { get; } // class/method name
        public DateTime Timestamp { get; }

        public LogEntry(LogLevel level, string message, string source)
        {
            Level = level;
            Message = message;
            Source = source;
            Timestamp = DateTime.Now;
        }

        public override string ToString() => $"[{Timestamp:HH:mm:ss}] [{Level}] [{Source}] {Message}";
    }
}