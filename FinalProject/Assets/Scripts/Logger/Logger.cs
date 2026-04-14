namespace Logger
{
    public static class Log
    {
        public static ILogger Default { get; set; } = new UnityLogger();
    }
}