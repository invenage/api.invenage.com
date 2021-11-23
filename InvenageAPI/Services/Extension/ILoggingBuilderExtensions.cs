using InvenageAPI.Services.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InvenageAPI.Services.Extension
{
    public static class ILoggingBuilderExtensions
    {
        public static ILoggingBuilder AddSystemLogger(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, SystemLoggerProvider>();
            return builder;
        }
    }
}
