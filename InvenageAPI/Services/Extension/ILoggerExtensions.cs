using Microsoft.Extensions.Logging;
using System;

namespace InvenageAPI.Services.Extension
{
    public static class ILoggerExtensions
    {
        public static void LogWarning(this ILogger logger, Exception error)
            => logger.LogWarning(new { error.Message, error.StackTrace }.ToJson());
        public static void LogError(this ILogger logger, Exception error)
            => logger.LogError(new { error.Message, error.StackTrace }.ToJson());
    }
}
