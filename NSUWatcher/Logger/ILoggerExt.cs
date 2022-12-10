namespace Microsoft.Extensions.Logging
{
    public static class LoggerExtensions
    {
        /// <summary>
        /// Create shoer name for logger context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        public static ILogger CreateLoggerShort<T>(this ILoggerFactory loggerFactory)
        {
            return loggerFactory.CreateLogger(typeof(T).Name);
        }
    }
}
