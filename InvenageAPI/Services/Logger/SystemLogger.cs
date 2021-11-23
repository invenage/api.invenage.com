using InvenageAPI.Models;
using InvenageAPI.Services.Extension;
using InvenageAPI.Services.Global;
using InvenageAPI.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace InvenageAPI.Services.Logger
{
    public class SystemLogger : ILogger
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _accessor;
        private readonly IStorage _archiveStorage;
        private readonly IStorage _localStorage;
        private readonly string _categoryName;

        public SystemLogger(IConfiguration config, IHttpContextAccessor accessor, ILocalStorage localStorage, IArchiveStorage archiveStorage, string categoryName)
        {
            _config = config;
            _accessor = accessor;
            _localStorage = localStorage;
            _archiveStorage = archiveStorage;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => logLevel >= GetMinLogLevelFromConfig(_config);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var record = new LogData
            {
                TraceId = _accessor.HttpContext?.TraceIdentifier ?? "system",
                RecordTime = GlobalVariable.CurrentTime,
                CategoryName = _categoryName,
                Level = logLevel.ToString(),
                Message = formatter(state, exception),
            };
            WriteMessageToStorage(record, _archiveStorage, _localStorage, _config);
        }

        private static LogLevel GetMinLogLevelFromConfig(IConfiguration config)
            => (config.GetValue<string>("Logging:LogLevel:Default") ?? "Trace").PraseAsEnum<LogLevel>();

        public static void WriteMessageToStorage(LogData model, IStorage mainStorage, IStorage backupStorage, IConfiguration config)
        {
            if (model.Level.PraseAsEnum<LogLevel>() < GetMinLogLevelFromConfig(config))
                return;

            var datum = TrimModelSize(model);
            if (!GlobalVariable.IsLocal)
            {
                try
                {
                    foreach (LogData data in datum)
                        TaskExtensions.RunTask(async () => await mainStorage.SaveAsync("Log", "APILog", data));
                }
                catch (Exception mainStorageException)
                {
                    try
                    {
                        model.Message = (new { mainStorageException.Message, mainStorageException.StackTrace }).ToJson();
                        model.Id = Guid.NewGuid().ToString();
                        TaskExtensions.RunTask(async () => await backupStorage.SaveAsync("Log", "APILog", model));
                    }
                    catch (Exception backupStorageException)
                    {
                        Console.WriteLine(backupStorageException);
                    }
                }
            }

            foreach (LogData data in datum)
                TaskExtensions.RunTask(() => Console.WriteLine(data.ToJson()));
        }

        private static IEnumerable<LogData> TrimModelSize(LogData model)
        {
            var maxLength = 1000000;
            var originMessage = model.Message;
            if (originMessage.Length > maxLength)
            {
                int i = 0;
                while (i <= originMessage.Length)
                {
                    var newModel = new LogData()
                    {
                        Id = Guid.NewGuid().ToString(),
                        CategoryName = model.CategoryName,
                        Level = model.Level,
                        Message = originMessage.Substring(i, i + maxLength > originMessage.Length ? originMessage.Length - i : maxLength),
                        RecordTime = model.RecordTime,
                        TraceId = model.TraceId
                    };
                    yield return newModel;
                    i += maxLength;
                }
                yield break;
            }
            yield return model;
        }
    }
}
