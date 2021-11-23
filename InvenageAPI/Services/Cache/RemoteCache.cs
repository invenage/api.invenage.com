using InvenageAPI.Services.Enum;
using InvenageAPI.Services.Extension;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;

namespace InvenageAPI.Services.Cache
{
    public class RemoteCache : IRemoteCache
    {
        private readonly ConnectionMultiplexer multiplexer;
        private readonly ILogger<ICache> _logger;

        public RemoteCache(IConfiguration config, ILogger<RemoteCache> logger)
        {
            if (multiplexer == null)
                multiplexer = ConnectionMultiplexer.Connect(config.GetConnectionString("RemoteCache"));
            _logger = logger;
        }

        public CacheType GetCacheType()
            => CacheType.Remote;

        public bool Set<T>(string key, T value, int expiresMinutes = 10)
        {
            try
            {
                var database = multiplexer.GetDatabase();
                expiresMinutes = Math.Max(expiresMinutes, 10);
                database.StringSet(key, value.ToJson(), TimeSpan.FromMinutes(expiresMinutes));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

            return false;
        }

        public bool Get<T>(string key, out T result)
        {
            result = default;
            try
            {
                var database = multiplexer.GetDatabase();
                if (key.IsNullOrEmpty() || database == null || !database.KeyExists(key))
                    return false;

                var data = database.StringGet(key);
                if (data.HasValue)
                {
                    result = data.ToString().FromJson<T>();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

            return false;
        }

        public bool Remove(string key)
        {
            try
            {
                var database = multiplexer.GetDatabase();
                if (!database.KeyExists(key))
                    return true;

                database.KeyDelete(key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

            return false;
        }

        public bool Clear(long timeStamp, bool clearRemote)
        {
            try
            {
                var endPoints = multiplexer.GetEndPoints(true);
                foreach (var endPoint in endPoints)
                {
                    multiplexer.GetServer(endPoint).FlushAllDatabases();
                }
                Set("lastClear", timeStamp, 60);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return false;
            }
            return true;
        }
    }
}
