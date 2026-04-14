using UnityEngine;
namespace Logger
{
    public class UnityLogger : ILogger
    {
        private readonly bool _logInBuild;

        public UnityLogger(bool logInBuild = false)
        {
            _logInBuild = logInBuild;
        }

        public void Log(LogEntry entry)
        {
            // Only log in Editor or if allowed in build
            if (!Application.isEditor && !_logInBuild)
                return;

            switch (entry.Level)
            {
                case LogLevel.Info:
                    Debug.Log(entry.ToString());
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(entry.ToString());
                    break;
                case LogLevel.Error:
                    Debug.LogError(entry.ToString());
                    break;
            }
        }
    }
}