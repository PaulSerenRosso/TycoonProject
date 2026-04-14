using System.Collections.Generic;
using System.Linq;
using Logger;

namespace Tests.Editor.EditorTestUtility
{
    public class TestLogger : ILogger
    {

   
        public List<LogEntry> LoggedEntries { get; } = new List<LogEntry>();

        public void Log(LogEntry entry)
        {
            LoggedEntries.Add(entry);
        }

        public bool HasErrorContaining()
        {
            return LoggedEntries.Any(e =>
                e.Level == LogLevel.Error);
        }
    }
}