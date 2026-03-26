using System.Collections.Generic;
using Logger;


namespace Tests.Editor
{
    public class TestLogger : ILogger
    {
        public List<LogEntry> Logs = new();

        public void Log(LogEntry entry)
        {
            Logs.Add(entry);
        }
    }
}