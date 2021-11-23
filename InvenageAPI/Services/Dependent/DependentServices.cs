using InvenageAPI.Services.Cache;
using InvenageAPI.Services.Connection;
using InvenageAPI.Services.Enum;
using InvenageAPI.Services.Global;
using InvenageAPI.Services.Storage;
using InvenageAPI.Services.Synchronizer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace InvenageAPI.Services.Dependent
{
    public class DependentServices : IDependent
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ICache _localCache;
        private readonly ICache _remoteCache;
        private readonly IStorage _localStorage;
        private readonly IStorage _remoteStorage;
        private readonly IStorage _archiveStorage;
        private readonly ICacheSynchronizer _cacheSynchronizer;
        private readonly IStorageSynchronizer _storageSynchronizer;
        private readonly IExternalConnection _apiConnection;

        public DependentServices(IConfiguration config, IHttpContextAccessor accessor, IHostApplicationLifetime lifetime,
            ILocalCache localCache, IRemoteCache remoteCache,
            ILocalStorage localStorage, IRemoteStorage remoteStorage, IArchiveStorage archiveStorage,
            ICacheSynchronizer cacheSynchronizer, IStorageSynchronizer storageSynchronizer,
            IApiConnection apiConnection)
        {
            _config = config;
            _accessor = accessor;
            _lifetime = lifetime;
            _localCache = localCache;
            _remoteCache = remoteCache;
            _localStorage = localStorage;
            _remoteStorage = remoteStorage;
            _archiveStorage = archiveStorage;
            _cacheSynchronizer = cacheSynchronizer;
            _storageSynchronizer = storageSynchronizer;
            _apiConnection = apiConnection;
        }

        public ICache GetCache()
            => _localCache;

        public ICache GetCache(CacheType type)
            => type switch
            {
                CacheType.Local => _localCache,
                CacheType.Remote => _remoteCache,
                _ => throw new NotSupportedException(),
            };

        public IStorage GetStorage()
            => GlobalVariable.IsLocal ? _localStorage : _remoteStorage;

        public IStorage GetStorage(StorageType type)
            => type switch
            {
                StorageType.Local => _localStorage,
                StorageType.Remote => _remoteStorage,
                StorageType.Archive => _archiveStorage,
                _ => throw new NotSupportedException(),
            };

        public ISynchronizer GetSynchronizer(SynchronizerType type)
            => type switch
            {
                SynchronizerType.Cache => _cacheSynchronizer,
                SynchronizerType.Storage => _storageSynchronizer,
                _ => throw new NotSupportedException(),
            };

        public T GetConfig<T>(string key)
            => _config.GetValue<T>(key);

        public string GetConnectionString(string key)
            => _config.GetConnectionString(key);

        public ILogger GetLogger<T>()
            => GetService<ILogger<T>>();

        public T GetService<T>()
            => (T)_accessor.HttpContext?.RequestServices.GetService(typeof(T)) ?? throw new NotSupportedException();

        public IHostApplicationLifetime GetHostApplicationLifetime()
            => _lifetime;

        public IExternalConnection GetConnection(ConnectionType type)
            => type switch
            {
                ConnectionType.API => _apiConnection,
                _ => throw new NotSupportedException(),
            };
    }
}
