using InvenageAPI.Models;
using InvenageAPI.Services.Cache;
using InvenageAPI.Services.Enum;
using InvenageAPI.Services.Extension;
using InvenageAPI.Services.Global;
using InvenageAPI.Services.Logger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace InvenageAPI.Services.Synchronizer
{
    public class CacheSynchronizer : ICacheSynchronizer
    {
        private readonly ILogger _logger;
        private ICache _localCache;
        private readonly ICache _remoteCache;
        private readonly ConcurrentQueue<CacheQueueModel> insertQueue;
        private readonly ConcurrentQueue<string> updateQueue;
        private readonly ConcurrentQueue<string> deleteQueue;
        private Timer timer;
        private readonly int interval;

        public CacheSynchronizer(IConfiguration config, IRemoteCache remoteCache, ILogger<CacheSynchronizer> logger)
        {
            _logger = logger;
            _remoteCache = remoteCache;
            insertQueue = new();
            updateQueue = new();
            deleteQueue = new();
            interval = config.GetValue<int>("Synchronizer:Interval");

            _logger.LogTrace("CacheSynchronizer constructed");
            if (!GlobalVariable.IsLocal)
                Initiate();
        }

        public SynchronizerType GetSynchronizerType()
            => SynchronizerType.Cache;

        public void Queue<T>(string key, T value, int expiresMinutes = 5)
        {
            insertQueue.Enqueue(new() { Key = key, Value = value, ExpiresMinutes = expiresMinutes });
            _logger.LogTrace("CacheSynchronizer Queue 1 new item");
        }

        public void QueueUpdate(string key)
        {
            updateQueue.Enqueue(key);
            _logger.LogTrace("CacheSynchronizer QueueUpdate 1 new item");
        }

        public void QueueDelete(string key)
        {
            deleteQueue.Enqueue(key);
            _logger.LogTrace("CacheSynchronizer QueueDelete 1 new item");
        }

        public bool Process()
        {
            try
            {
                if (_localCache == null)
                    SetupLocalCache();

                if (CheckClear())
                {
                    insertQueue.Clear();
                    updateQueue.Clear();
                    deleteQueue.Clear();
                    return true;
                }

                _logger.LogTrace("CacheSynchronizer Start Process");
                while (insertQueue.TryDequeue(out var result))
                {
                    _remoteCache.Set(result.Key, result.Value, result.ExpiresMinutes);
                    _logger.LogTrace("CacheSynchronizer Dequeue 1 item");
                }
                while (updateQueue.TryDequeue(out var result))
                {
                    SyncKey(result);
                    _logger.LogTrace("CacheSynchronizer DequeueUpdate 1 item");
                }
                while (deleteQueue.TryDequeue(out var result))
                {
                    _remoteCache.Remove(result);
                    _logger.LogTrace("CacheSynchronizer DequeueDelete 1 item");
                }

                _logger.LogTrace("CacheSynchronizer End Process");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"CacheSynchronizer Error: {e.Message} {e.StackTrace}");
                Stop();
            }
            return false;
        }

        public void Stop()
        {
            if (timer != null)
                timer.Dispose();
            _logger.LogTrace("CacheSynchronizer Stopped");
        }

        public void Initiate()
        {
            if (timer != null)
                timer.Dispose();
            timer = new((e) => Process(), null, TimeSpan.Zero, TimeSpan.FromSeconds(interval));
            _logger.LogTrace("CacheSynchronizer Initiated");
        }

        private void SetupLocalCache()
        {
            SystemLoggerProvider.Loggers.TryGetValue("LocalCache", out var localLog);
            _localCache = new LocalCache((IRemoteCache)_remoteCache, (ILogger<LocalCache>)localLog, this);
        }

        private void SyncKey(string key)
        {
            _remoteCache.Get(key, out long remoteValue);
            _localCache.Get(key, out long localValue);
            if (remoteValue != localValue)
                _localCache.Set(key, remoteValue);
        }

        private bool CheckClear()
        {
            var haveClear = false;
            _remoteCache.Get("lastClear", out long remoteLastClear);
            _localCache.Get("lastClear", out long localLastClear);
            _logger.LogTrace($"CacheSynchronizer lastClear compare: remote {remoteLastClear}, local {localLastClear}");
            if (localLastClear != remoteLastClear)
            {
                _localCache.Clear(remoteLastClear, false);
                _logger.LogTrace($"CacheSynchronizer clear executed");
                haveClear = true;
            }
            return haveClear;
        }
    }

    internal class CacheQueueModel
    {
        public string Key;
        public object Value;
        public int ExpiresMinutes;
    }
}
