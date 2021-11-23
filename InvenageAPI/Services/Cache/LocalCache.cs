using InvenageAPI.Services.Enum;
using InvenageAPI.Services.Extension;
using InvenageAPI.Services.Synchronizer;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Runtime.Caching;

namespace InvenageAPI.Services.Cache
{
    public class LocalCache : ILocalCache
    {
        private readonly MemoryCache _cache;
        private readonly ICache _remoteCache;
        private readonly ILogger _logger;
        private readonly ICacheSynchronizer _synchronizer;

        public LocalCache(IRemoteCache remoteCache, ILogger<LocalCache> logger, ICacheSynchronizer synchronizer)
        {
            if (_cache == null)
                _cache = MemoryCache.Default;
            _logger = logger;
            _synchronizer = synchronizer;
            _remoteCache = remoteCache;
        }

        public CacheType GetCacheType()
            => CacheType.Local;

        public bool Get<T>(string key, out T result)
        {
            result = default;
            try
            {
                if (key.IsNullOrEmpty() || _cache == null)
                    return false;
                if (!_cache.Contains(key))
                {
                    if (CheckRemoteCache(key, out result))
                    {
                        return Set(key, result);
                    }
                    return false;
                }
                result = (T)_cache.Get(key);
                _synchronizer.QueueUpdate(key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

            return false;
        }

        public bool Set<T>(string key, T value, int expiresMinutes = 5)
        {
            try
            {
                _cache.Set(new(key, value), new()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(expiresMinutes)
                });
                _synchronizer.Queue(key, value, expiresMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return false;
            }

            return true;
        }

        public bool Remove(string key)
        {
            try
            {
                _cache.Remove(key);
                _synchronizer.QueueDelete(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return false;
            }
            return true;
        }

        public bool Clear(long timeStamp, bool clearRemote)
        {
            try
            {
                foreach (var key in _cache.Select(x => x.Key).ToList())
                {
                    _cache.Remove(key);
                }
                Set("lastClear", timeStamp, 60);

                if (clearRemote)
                    ClearRemoteCache(timeStamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return false;
            }
            return true;
        }

        private bool CheckRemoteCache<T>(string key, out T result)
        {
            result = default;
            try
            {
                return _remoteCache.Get(key, out result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

            return false;
        }

        private bool ClearRemoteCache(long timeStamp)
        {
            try
            {
                return _remoteCache.Clear(timeStamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

            return false;
        }
    }
}
