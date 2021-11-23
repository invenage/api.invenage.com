using InvenageAPI.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace InvenageAPI.Services.Logger
{
    public class SystemLoggerProvider : ILoggerProvider
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILocalStorage _localStorage;
        private readonly IArchiveStorage _archiveStorage;
        public static readonly ConcurrentDictionary<string, ILogger> Loggers = new();

        public SystemLoggerProvider(IConfiguration config, IHttpContextAccessor accessor, ILocalStorage localStorage, IArchiveStorage archiveStorage)
        {
            _config = config;
            _accessor = accessor;
            _localStorage = localStorage;
            _archiveStorage = archiveStorage;
        }

        public ILogger CreateLogger(string categoryName)
            => Loggers.GetOrAdd(categoryName, new SystemLogger(_config, _accessor, _localStorage, _archiveStorage, categoryName));

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Loggers.Clear();
        }
    }
}
