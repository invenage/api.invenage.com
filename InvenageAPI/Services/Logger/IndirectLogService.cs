using InvenageAPI.Models;
using InvenageAPI.Services.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InvenageAPI.Services.Logger
{
    public class IndirectLogService
    {
        private readonly IStorage _mainStorage;
        private readonly IStorage _backupStorage;
        private readonly IConfiguration _config;

        public IndirectLogService(IStorage mainStorage, IStorage backupStorage, IConfiguration config)
        {
            _mainStorage = mainStorage;
            _backupStorage = backupStorage;
            _config = config;
        }

        public void WriteLog(LogLevel logLevel, string categoryName, string message)
        {
            var record = new LogData
            {
                TraceId = "system",
                CategoryName = categoryName,
                Level = logLevel.ToString(),
                Message = message,
            };
            SystemLogger.WriteMessageToStorage(record, _mainStorage, _backupStorage, _config);
        }

        public void WriteTrace(object type, string message)
            => WriteLog(LogLevel.Trace, type.GetType().FullName, message);
        public void WriteDebug(object type, string message)
            => WriteLog(LogLevel.Debug, type.GetType().FullName, message);
        public void WriteInformation(object type, string message)
            => WriteLog(LogLevel.Information, type.GetType().FullName, message);
        public void WriteWarning(object type, string message)
            => WriteLog(LogLevel.Warning, type.GetType().FullName, message);
        public void WriteError(object type, string message)
            => WriteLog(LogLevel.Error, type.GetType().FullName, message);
    }
}
