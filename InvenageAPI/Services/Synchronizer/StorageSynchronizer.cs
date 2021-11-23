using InvenageAPI.Models;
using InvenageAPI.Services.Enum;
using InvenageAPI.Services.Extension;
using InvenageAPI.Services.Global;
using InvenageAPI.Services.Logger;
using InvenageAPI.Services.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace InvenageAPI.Services.Synchronizer
{
    public class StorageSynchronizer : IStorageSynchronizer
    {
        private readonly IStorage _remoteStorage;
        private readonly ConcurrentQueue<StorageQueueModel> queue;
        private readonly ConcurrentQueue<StorageQueueModel> delQueue;
        private Timer timer;
        private readonly int interval;
        private readonly IndirectLogService logService;
        public StorageSynchronizer(IConfiguration config, IRemoteStorage remoteStorage, IArchiveStorage archiveStorage)
        {
            logService = new(archiveStorage, remoteStorage, config);
            _remoteStorage = remoteStorage;
            queue = new();
            delQueue = new();
            interval = config.GetValue<int>("Synchronizer:Interval");
            logService.WriteTrace(this, "StorageSynchronizer constructed");
            if (!GlobalVariable.IsLocal)
                Initiate();
        }

        public SynchronizerType GetSynchronizerType()
            => SynchronizerType.Storage;

        public void Queue<T>(string database, string collection, T data) where T : DataModel
        {
            queue.Enqueue(new() { Database = database, Collection = collection, Data = data });
            logService.WriteTrace(this, "StorageSynchronizer Queue 1 new item");
        }

        public void QueueDelete(string database, string collection, string id)
        {
            delQueue.Enqueue(new() { Database = database, Collection = collection, Id = id });
            logService.WriteTrace(this, "StorageSynchronizer QueueDelete 1 new item");
        }

        public bool Process()
        {
            try
            {
                logService.WriteTrace(this, "StorageSynchronizer Start Process");
                while (queue.TryDequeue(out var result))
                {
                    TaskExtensions.RunTask(async () => await _remoteStorage.SaveAsync(result.Database, result.Collection, result.Data));
                    logService.WriteTrace(this, "StorageSynchronizer Dequeue 1 item");
                }
                while (delQueue.TryDequeue(out var result))
                {
                    TaskExtensions.RunTask(async () => await _remoteStorage.DelectAsync(result.Database, result.Collection, result.Id));
                    logService.WriteTrace(this, "StorageSynchronizer DequeueDelete 1 item");
                }
                logService.WriteTrace(this, "StorageSynchronizer End Process");
                return true;
            }
            catch (Exception e)
            {
                logService.WriteWarning(this, $"StorageSynchronizer Error: {e.Message} {e.StackTrace}");
                Stop();
            }
            return false;
        }

        public void Stop()
        {
            if (timer != null)
                timer.Dispose();
            logService.WriteTrace(this, "StorageSynchronizer Stopped");
        }

        public void Initiate()
        {
            if (timer != null)
                timer.Dispose();
            timer = new((e) => Process(), null, TimeSpan.Zero, TimeSpan.FromSeconds(interval));
            logService.WriteTrace(this, "StorageSynchronizer Initiated");
        }
    }

    internal class StorageQueueModel
    {
        public string Database;
        public string Collection;
        public string Id;
        public DataModel Data;
    }
}
