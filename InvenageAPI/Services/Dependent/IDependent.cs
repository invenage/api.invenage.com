using InvenageAPI.Services.Cache;
using InvenageAPI.Services.Connection;
using InvenageAPI.Services.Enum;
using InvenageAPI.Services.Storage;
using InvenageAPI.Services.Synchronizer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvenageAPI.Services.Dependent
{
    public interface IDependent
    {
        public ICache GetCache();
        public ICache GetCache(CacheType type);
        public IStorage GetStorage();
        public IStorage GetStorage(StorageType type);
        public ISynchronizer GetSynchronizer(SynchronizerType type);
        public T GetConfig<T>(string key);
        public string GetConnectionString(string key);
        public ILogger GetLogger<T>();
        public T GetService<T>();
        public IHostApplicationLifetime GetHostApplicationLifetime();
        public IExternalConnection GetConnection(ConnectionType type);
    }
}
